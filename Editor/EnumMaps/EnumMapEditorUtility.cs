#if UNITY_EDITOR

using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace SmartPools.EnumMaps.Editor
{
    internal static class EnumMapEditorUtility
    {
        public static EnumMapEditorAnalysis Analyze(
            SerializedProperty entriesProperty,
            bool requirePersistentObjectReference)
        {
            EnumMapEditorAnalysis analysis = new EnumMapEditorAnalysis();

            if (entriesProperty == null)
            {
                analysis.AddError(-1, "EnumMap entries property could not be found.");
                return analysis;
            }

            Dictionary<int, List<int>> indicesByKeyValue = new Dictionary<int, List<int>>();

            for (int i = 0; i < entriesProperty.arraySize; i++)
            {
                SerializedProperty entryProperty = entriesProperty.GetArrayElementAtIndex(i);

                SerializedProperty keyProperty =
                    entryProperty.FindPropertyRelative(EnumMapBase.KeyFieldName);

                SerializedProperty valueProperty =
                    entryProperty.FindPropertyRelative(EnumMapBase.ValueFieldName);

                if (keyProperty == null)
                {
                    analysis.AddError(i, $"Entry {i} has no key property.");
                    continue;
                }

                string keyText = GetKeyDisplayText(keyProperty);
                int keyValue = keyProperty.intValue;

                if (keyProperty.propertyType != SerializedPropertyType.Enum)
                {
                    analysis.AddError(
                        i,
                        $"Entry {i} key is not serialized as an enum.");

                    continue;
                }

                if (keyProperty.enumValueIndex < 0 ||
                    keyProperty.enumValueIndex >= keyProperty.enumDisplayNames.Length)
                {
                    analysis.AddError(
                        i,
                        $"Entry {i} uses an undefined enum value '{keyText}'.");
                }

                List<int> indices;

                if (!indicesByKeyValue.TryGetValue(keyValue, out indices))
                {
                    indices = new List<int>();
                    indicesByKeyValue.Add(keyValue, indices);
                }

                indices.Add(i);

                if (valueProperty == null)
                {
                    analysis.AddError(i, $"Entry {i} has no value property.");
                    continue;
                }

                if (valueProperty.propertyType != SerializedPropertyType.ObjectReference)
                {
                    analysis.AddError(
                        i,
                        $"Entry {i} value is not an object reference.");

                    continue;
                }

                Object value = valueProperty.objectReferenceValue;

                if (value == null)
                {
                    analysis.AddError(
                        i,
                        $"Entry {i} with key '{keyText}' has no value assigned.");

                    continue;
                }

                if (requirePersistentObjectReference && !EditorUtility.IsPersistent(value))
                {
                    analysis.AddError(
                        i,
                        $"Entry {i} with key '{keyText}' references a scene object. Use a prefab or asset reference instead.");
                }
            }

            foreach (KeyValuePair<int, List<int>> pair in indicesByKeyValue)
            {
                if (pair.Value.Count <= 1)
                    continue;

                string keyText = GetKeyDisplayText(entriesProperty, pair.Value[0]);
                string indicesText = string.Join(", ", pair.Value);

                for (int i = 0; i < pair.Value.Count; i++)
                {
                    analysis.AddError(
                        pair.Value[i],
                        $"Duplicate enum key '{keyText}' appears at entries {indicesText}.");
                }
            }

            return analysis;
        }

        public static int GetFirstUnusedEnumIndex(
            SerializedProperty entriesProperty,
            int excludedIndex,
            SerializedProperty keyPrototypeProperty)
        {
            if (keyPrototypeProperty == null ||
                keyPrototypeProperty.propertyType != SerializedPropertyType.Enum ||
                keyPrototypeProperty.enumDisplayNames == null ||
                keyPrototypeProperty.enumDisplayNames.Length == 0)
            {
                return 0;
            }

            HashSet<int> usedEnumIndices = new HashSet<int>();

            if (entriesProperty != null)
            {
                for (int i = 0; i < entriesProperty.arraySize; i++)
                {
                    if (i == excludedIndex)
                        continue;

                    SerializedProperty entryProperty = entriesProperty.GetArrayElementAtIndex(i);

                    SerializedProperty keyProperty =
                        entryProperty.FindPropertyRelative(EnumMapBase.KeyFieldName);

                    if (keyProperty == null ||
                        keyProperty.propertyType != SerializedPropertyType.Enum)
                    {
                        continue;
                    }

                    if (keyProperty.enumValueIndex >= 0)
                        usedEnumIndices.Add(keyProperty.enumValueIndex);
                }
            }

            int enumCount = keyPrototypeProperty.enumDisplayNames.Length;

            for (int i = 0; i < enumCount; i++)
            {
                if (!usedEnumIndices.Contains(i))
                    return i;
            }

            return 0;
        }

        public static void AddMissingEnumValues(SerializedProperty entriesProperty)
        {
            if (entriesProperty == null)
                return;

            SerializedProperty prototypeKeyProperty = GetOrCreatePrototypeKeyProperty(entriesProperty);

            if (prototypeKeyProperty == null ||
                prototypeKeyProperty.propertyType != SerializedPropertyType.Enum ||
                prototypeKeyProperty.enumDisplayNames == null)
            {
                entriesProperty.serializedObject.ApplyModifiedProperties();
                return;
            }

            int enumCount = prototypeKeyProperty.enumDisplayNames.Length;
            HashSet<int> usedEnumIndices = GetUsedEnumIndices(entriesProperty);

            for (int enumIndex = 0; enumIndex < enumCount; enumIndex++)
            {
                if (usedEnumIndices.Contains(enumIndex))
                    continue;

                int newIndex = entriesProperty.arraySize;
                entriesProperty.arraySize++;

                SerializedProperty newEntry = entriesProperty.GetArrayElementAtIndex(newIndex);
                SerializedProperty keyProperty =
                    newEntry.FindPropertyRelative(EnumMapBase.KeyFieldName);

                SerializedProperty valueProperty =
                    newEntry.FindPropertyRelative(EnumMapBase.ValueFieldName);

                if (keyProperty != null &&
                    keyProperty.propertyType == SerializedPropertyType.Enum)
                {
                    keyProperty.enumValueIndex = enumIndex;
                }

                if (valueProperty != null &&
                    valueProperty.propertyType == SerializedPropertyType.ObjectReference)
                {
                    valueProperty.objectReferenceValue = null;
                }
            }

            entriesProperty.serializedObject.ApplyModifiedProperties();
        }

        public static string GetKeyDisplayText(SerializedProperty keyProperty)
        {
            if (keyProperty == null)
                return "<missing>";

            if (keyProperty.propertyType != SerializedPropertyType.Enum)
                return keyProperty.intValue.ToString();

            int index = keyProperty.enumValueIndex;

            if (index >= 0 &&
                keyProperty.enumDisplayNames != null &&
                index < keyProperty.enumDisplayNames.Length)
            {
                return keyProperty.enumDisplayNames[index];
            }

            return keyProperty.intValue.ToString();
        }

        private static string GetKeyDisplayText(SerializedProperty entriesProperty, int index)
        {
            if (entriesProperty == null ||
                index < 0 ||
                index >= entriesProperty.arraySize)
            {
                return "<missing>";
            }

            SerializedProperty entryProperty = entriesProperty.GetArrayElementAtIndex(index);

            SerializedProperty keyProperty =
                entryProperty.FindPropertyRelative(EnumMapBase.KeyFieldName);

            return GetKeyDisplayText(keyProperty);
        }

        private static SerializedProperty GetOrCreatePrototypeKeyProperty(
            SerializedProperty entriesProperty)
        {
            if (entriesProperty.arraySize == 0)
                entriesProperty.arraySize = 1;

            SerializedProperty firstEntry = entriesProperty.GetArrayElementAtIndex(0);

            return firstEntry.FindPropertyRelative(EnumMapBase.KeyFieldName);
        }

        private static HashSet<int> GetUsedEnumIndices(SerializedProperty entriesProperty)
        {
            HashSet<int> usedEnumIndices = new HashSet<int>();

            for (int i = 0; i < entriesProperty.arraySize; i++)
            {
                SerializedProperty entryProperty = entriesProperty.GetArrayElementAtIndex(i);

                SerializedProperty keyProperty =
                    entryProperty.FindPropertyRelative(EnumMapBase.KeyFieldName);

                if (keyProperty == null ||
                    keyProperty.propertyType != SerializedPropertyType.Enum)
                {
                    continue;
                }

                if (keyProperty.enumValueIndex >= 0)
                    usedEnumIndices.Add(keyProperty.enumValueIndex);
            }

            return usedEnumIndices;
        }
    }
}

#endif

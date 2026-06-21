#if UNITY_EDITOR

using System.Collections.Generic;
using UnityEditor;
using UnityEditorInternal;
using UnityEngine;

namespace SmartPools.EnumMaps.Editor
{
    [CustomPropertyDrawer(typeof(EnumMapBase), true)]
    internal sealed class EnumMapDrawer : PropertyDrawer
    {
        private const float ColumnGap = 4f;
        private const float MinKeyWidth = 110f;
        private const float MaxKeyWidth = 180f;
        private const float HeaderButtonWidth = 112f;

        private readonly Dictionary<string, EnumMapDrawerState> _states =
            new Dictionary<string, EnumMapDrawerState>();

        public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
        {
            SerializedProperty entriesProperty =
                property.FindPropertyRelative(EnumMapBase.EntriesFieldName);

            if (entriesProperty == null)
                return EditorGUIUtility.singleLineHeight * 2f;

            ReorderableList list = GetList(property, entriesProperty, label);
            EnumMapEditorAnalysis analysis =
                EnumMapEditorUtility.Analyze(entriesProperty, true);

            float height = list.GetHeight();

            if (analysis.HasIssues)
            {
                string message = analysis.ToSummaryMessage();
                float helpHeight = EditorStyles.helpBox.CalcHeight(
                    new GUIContent(message),
                    EditorGUIUtility.currentViewWidth);

                height += helpHeight;
                height += EditorGUIUtility.standardVerticalSpacing;
            }

            return height;
        }

        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            SerializedProperty entriesProperty =
                property.FindPropertyRelative(EnumMapBase.EntriesFieldName);

            if (entriesProperty == null)
            {
                EditorGUI.HelpBox(
                    position,
                    $"Could not find '{EnumMapBase.EntriesFieldName}' on EnumMap.",
                    MessageType.Error);

                return;
            }

            ReorderableList list = GetList(property, entriesProperty, label);
            EnumMapEditorAnalysis analysis =
                EnumMapEditorUtility.Analyze(entriesProperty, true);

            Rect listRect = position;
            listRect.height = list.GetHeight();

            list.DoList(listRect);

            if (!analysis.HasIssues)
                return;

            string message = analysis.ToSummaryMessage();
            float helpHeight = EditorStyles.helpBox.CalcHeight(
                new GUIContent(message),
                position.width);

            Rect helpRect = new Rect(
                position.x,
                listRect.yMax + EditorGUIUtility.standardVerticalSpacing,
                position.width,
                helpHeight);

            EditorGUI.HelpBox(
                helpRect,
                message,
                analysis.HasErrors ? MessageType.Error : MessageType.Warning);
        }

        private ReorderableList GetList(
            SerializedProperty property,
            SerializedProperty entriesProperty,
            GUIContent label)
        {
            string key = property.serializedObject.targetObject.GetInstanceID() +
                         ":" +
                         property.propertyPath;

            EnumMapDrawerState state;

            if (!_states.TryGetValue(key, out state) || state.List == null)
            {
                state = new EnumMapDrawerState
                {
                    PropertyKey = key
                };

                state.List = CreateList(entriesProperty, label);
                _states[key] = state;
            }

            state.List.serializedProperty = entriesProperty;
            return state.List;
        }

        private ReorderableList CreateList(
            SerializedProperty entriesProperty,
            GUIContent label)
        {
            ReorderableList list = new ReorderableList(
                entriesProperty.serializedObject,
                entriesProperty,
                true,
                true,
                true,
                true);

            list.drawHeaderCallback = rect =>
            {
                Rect labelRect = rect;
                labelRect.xMax -= HeaderButtonWidth + ColumnGap;

                EditorGUI.LabelField(labelRect, label);

                Rect buttonRect = new Rect(
                    rect.xMax - HeaderButtonWidth,
                    rect.y + 1f,
                    HeaderButtonWidth,
                    EditorGUIUtility.singleLineHeight - 2f);

                if (GUI.Button(buttonRect, "Add Missing", EditorStyles.miniButton))
                {
                    EnumMapEditorUtility.AddMissingEnumValues(list.serializedProperty);
                }
            };

            list.elementHeightCallback = index =>
            {
                return EditorGUIUtility.singleLineHeight + 4f;
            };

            list.drawElementCallback = (rect, index, isActive, isFocused) =>
            {
                DrawElement(rect, list.serializedProperty, index);
            };

            list.onAddCallback = targetList =>
            {
                SerializedProperty entries = targetList.serializedProperty;

                int newIndex = entries.arraySize;
                entries.arraySize++;

                SerializedProperty newEntry = entries.GetArrayElementAtIndex(newIndex);

                SerializedProperty keyProperty =
                    newEntry.FindPropertyRelative(EnumMapBase.KeyFieldName);

                SerializedProperty valueProperty =
                    newEntry.FindPropertyRelative(EnumMapBase.ValueFieldName);

                if (keyProperty != null &&
                    keyProperty.propertyType == SerializedPropertyType.Enum)
                {
                    keyProperty.enumValueIndex =
                        EnumMapEditorUtility.GetFirstUnusedEnumIndex(
                            entries,
                            newIndex,
                            keyProperty);
                }

                if (valueProperty != null &&
                    valueProperty.propertyType == SerializedPropertyType.ObjectReference)
                {
                    valueProperty.objectReferenceValue = null;
                }

                entries.serializedObject.ApplyModifiedProperties();
            };

            list.onRemoveCallback = targetList =>
            {
                ReorderableList.defaultBehaviours.DoRemoveButton(targetList);
                targetList.serializedProperty.serializedObject.ApplyModifiedProperties();
            };

            return list;
        }

        private void DrawElement(
            Rect rect,
            SerializedProperty entriesProperty,
            int index)
        {
            if (index < 0 || index >= entriesProperty.arraySize)
                return;

            EnumMapEditorAnalysis analysis =
                EnumMapEditorUtility.Analyze(entriesProperty, true);

            SerializedProperty entryProperty = entriesProperty.GetArrayElementAtIndex(index);

            SerializedProperty keyProperty =
                entryProperty.FindPropertyRelative(EnumMapBase.KeyFieldName);

            SerializedProperty valueProperty =
                entryProperty.FindPropertyRelative(EnumMapBase.ValueFieldName);

            Rect backgroundRect = rect;
            backgroundRect.x -= 4f;
            backgroundRect.width += 8f;

            if (analysis.RowHasError(index))
            {
                EditorGUI.DrawRect(backgroundRect, new Color(1f, 0.2f, 0.2f, 0.18f));
            }
            else if (analysis.RowHasWarning(index))
            {
                EditorGUI.DrawRect(backgroundRect, new Color(1f, 0.75f, 0.2f, 0.14f));
            }

            rect.y += 2f;
            rect.height = EditorGUIUtility.singleLineHeight;

            float keyWidth = Mathf.Clamp(rect.width * 0.35f, MinKeyWidth, MaxKeyWidth);

            Rect keyRect = new Rect(
                rect.x,
                rect.y,
                keyWidth,
                rect.height);

            Rect valueRect = new Rect(
                keyRect.xMax + ColumnGap,
                rect.y,
                rect.width - keyWidth - ColumnGap,
                rect.height);

            if (keyProperty != null)
                EditorGUI.PropertyField(keyRect, keyProperty, GUIContent.none);

            if (valueProperty != null)
                EditorGUI.PropertyField(valueRect, valueProperty, GUIContent.none);
        }
    }
}

#endif

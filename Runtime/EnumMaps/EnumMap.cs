using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Object = UnityEngine.Object;

namespace SmartPools.EnumMaps
{
    [Serializable]
    public sealed class EnumMap<TEnum, TValue> :
        EnumMapBase,
        IReadOnlyDictionary<TEnum, TValue>,
        ISerializationCallbackReceiver
        where TEnum : struct, Enum
        where TValue : Object
    {
        [SerializeField] private List<EnumMapEntry<TEnum, TValue>> _entries =
            new List<EnumMapEntry<TEnum, TValue>>();

        [NonSerialized] private Dictionary<TEnum, TValue> _cache;
        [NonSerialized] private bool _cacheDirty = true;

        public override int SerializedCount => _entries != null ? _entries.Count : 0;

        public IReadOnlyList<EnumMapEntry<TEnum, TValue>> Entries => _entries;

        public IEnumerable<TEnum> Keys
        {
            get
            {
                EnsureCache();
                return _cache.Keys;
            }
        }

        public IEnumerable<TValue> Values
        {
            get
            {
                EnsureCache();
                return _cache.Values;
            }
        }

        public int Count
        {
            get
            {
                EnsureCache();
                return _cache.Count;
            }
        }

        public TValue this[TEnum key]
        {
            get
            {
                EnsureCache();
                return _cache[key];
            }
        }

        public bool ContainsKey(TEnum key)
        {
            EnsureCache();
            return _cache.ContainsKey(key);
        }

        public bool TryGetValue(TEnum key, out TValue value)
        {
            EnsureCache();
            return _cache.TryGetValue(key, out value);
        }

        public TValue GetOrDefault(TEnum key)
        {
            EnsureCache();

            TValue value;
            return _cache.TryGetValue(key, out value) ? value : null;
        }

        public IReadOnlyDictionary<TEnum, TValue> AsReadOnlyDictionary()
        {
            EnsureCache();
            return _cache;
        }

        public void Set(TEnum key, TValue value)
        {
            if (value == null)
                throw new ArgumentNullException(nameof(value), "EnumMap does not allow null values.");

            int foundIndex = -1;

            for (int i = 0; i < _entries.Count; i++)
            {
                EnumMapEntry<TEnum, TValue> entry = _entries[i];

                if (entry == null)
                    continue;

                if (!EqualityComparer<TEnum>.Default.Equals(entry.Key, key))
                    continue;

                if (foundIndex >= 0)
                {
                    throw new EnumMapException(
                        $"Cannot set key '{key}' because the map already contains duplicate serialized entries for this key.");
                }

                foundIndex = i;
            }

            if (foundIndex >= 0)
            {
                _entries[foundIndex].SetValue(value);
            }
            else
            {
                _entries.Add(new EnumMapEntry<TEnum, TValue>(key, value));
            }

            MarkDirty();
        }

        public bool Remove(TEnum key)
        {
            for (int i = 0; i < _entries.Count; i++)
            {
                EnumMapEntry<TEnum, TValue> entry = _entries[i];

                if (entry == null)
                    continue;

                if (!EqualityComparer<TEnum>.Default.Equals(entry.Key, key))
                    continue;

                _entries.RemoveAt(i);
                MarkDirty();
                return true;
            }

            return false;
        }

        public void Clear()
        {
            _entries.Clear();
            MarkDirty();
        }

        public override EnumMapValidationResult ValidateSerializedEntries()
        {
            EnumMapValidationResult result = new EnumMapValidationResult();

            if (_entries == null)
                return result;

            if (typeof(TEnum).IsDefined(typeof(FlagsAttribute), true))
            {
                result.AddWarning(
                    -1,
                    null,
                    $"EnumMap key type '{typeof(TEnum).Name}' has [Flags]. This is usually not recommended for one-to-one prefab maps.");
            }

            Dictionary<TEnum, List<int>> indicesByKey = new Dictionary<TEnum, List<int>>();

            for (int i = 0; i < _entries.Count; i++)
            {
                EnumMapEntry<TEnum, TValue> entry = _entries[i];

                if (entry == null)
                {
                    result.AddError(i, null, "Entry is null.");
                    continue;
                }

                TEnum key = entry.Key;
                string keyText = key.ToString();

                if (!Enum.IsDefined(typeof(TEnum), key))
                {
                    result.AddError(
                        i,
                        keyText,
                        $"Key '{keyText}' is not a defined value of enum '{typeof(TEnum).Name}'.");
                }

                List<int> indices;

                if (!indicesByKey.TryGetValue(key, out indices))
                {
                    indices = new List<int>();
                    indicesByKey.Add(key, indices);
                }

                indices.Add(i);

                if (entry.Value == null)
                {
                    result.AddError(
                        i,
                        keyText,
                        $"Value for key '{keyText}' is null.");
                }
            }

            foreach (KeyValuePair<TEnum, List<int>> pair in indicesByKey)
            {
                if (pair.Value.Count <= 1)
                    continue;

                string keyText = pair.Key.ToString();
                string indicesText = string.Join(", ", pair.Value);

                for (int i = 0; i < pair.Value.Count; i++)
                {
                    result.AddError(
                        pair.Value[i],
                        keyText,
                        $"Duplicate key '{keyText}'. Entries with this key: {indicesText}.");
                }
            }

            return result;
        }

        public override void ValidateOrThrow(Object context = null)
        {
            EnumMapValidationResult result = ValidateSerializedEntries();

            if (result.IsValid)
                return;

            string contextText = context != null
                ? $" on '{context.name}'"
                : string.Empty;

            throw new EnumMapException(
                result.ToMessage($"EnumMap<{typeof(TEnum).Name}, {typeof(TValue).Name}> validation failed{contextText}."));
        }

        public IEnumerator<KeyValuePair<TEnum, TValue>> GetEnumerator()
        {
            EnsureCache();
            return _cache.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        public void OnBeforeSerialize()
        {
        }

        public void OnAfterDeserialize()
        {
            MarkDirty();
        }

        private void EnsureCache()
        {
            if (!_cacheDirty && _cache != null)
                return;

            ValidateOrThrow();

            _cache = new Dictionary<TEnum, TValue>(_entries.Count);

            for (int i = 0; i < _entries.Count; i++)
            {
                EnumMapEntry<TEnum, TValue> entry = _entries[i];
                _cache.Add(entry.Key, entry.Value);
            }

            _cacheDirty = false;
        }

        private void MarkDirty()
        {
            _cacheDirty = true;
        }
    }
}

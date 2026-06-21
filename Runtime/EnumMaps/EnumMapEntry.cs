using System;
using UnityEngine;
using Object = UnityEngine.Object;

namespace SmartPools.EnumMaps
{
    [Serializable]
    public sealed class EnumMapEntry<TEnum, TValue>
        where TEnum : struct, Enum
        where TValue : Object
    {
        [SerializeField] private TEnum _key;
        [SerializeField] private TValue _value;

        public TEnum Key => _key;
        public TValue Value => _value;

        public EnumMapEntry()
        {
        }

        public EnumMapEntry(TEnum key, TValue value)
        {
            _key = key;
            _value = value;
        }

        internal void SetKey(TEnum key)
        {
            _key = key;
        }

        internal void SetValue(TValue value)
        {
            _value = value;
        }
    }
}

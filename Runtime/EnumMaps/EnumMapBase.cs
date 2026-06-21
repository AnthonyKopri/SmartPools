using UnityEngine;

namespace SmartPools.EnumMaps
{
    [System.Serializable]
    public abstract class EnumMapBase
    {
        public const string EntriesFieldName = "_entries";
        public const string KeyFieldName = "_key";
        public const string ValueFieldName = "_value";

        public abstract int SerializedCount { get; }

        public abstract EnumMapValidationResult ValidateSerializedEntries();

        public abstract void ValidateOrThrow(Object context = null);
    }
}

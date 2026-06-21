using System;

namespace SmartPools.EnumMaps
{
    public sealed class EnumMapException : Exception
    {
        public EnumMapException()
        {
        }

        public EnumMapException(string message) : base(message)
        {
        }

        public EnumMapException(string message, Exception innerException) : base(message, innerException)
        {
        }
    }
}

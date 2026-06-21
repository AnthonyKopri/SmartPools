using UnityEngine;

namespace SmartPools
{
    public abstract class MultiPoolable <TPool, TEnum> : Poolable<TPool> 
        where TPool : class
        where TEnum : struct, System.Enum
    {
        [SerializeField] protected TEnum distinctType;
        public virtual TEnum DistinctType => distinctType;
    }
}

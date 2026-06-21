using System.Collections.Generic;

namespace SmartPools
{
    public abstract class MultiFactory<TPool, TEnum> : Factory<TPool> 
        where TPool : Poolable<TPool> 
        where TEnum : struct, System.Enum
    {
        /// <summary>
        /// Gets the SmartMultiPool instance from the PoolLocator.
        /// </summary>
        /// <remarks>
        /// If you want to use specific functions of an implementation, don't forget to use "as".
        /// Example: MultiPool as CubePool
        /// </remarks>
        protected virtual SmartMultiPool<TPool, TEnum> MultiPool => PoolLocator.Get<TPool>() as SmartMultiPool<TPool, TEnum>;
        public Dictionary<TEnum, List<TPool>> ActiveItemLists => MultiPool.ActiveObjects;
    }
}

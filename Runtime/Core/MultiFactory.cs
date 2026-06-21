using System.Collections.Generic;
using AYellowpaper.SerializedCollections;

namespace SmartPools
{
    public abstract class MultiFactory<TPool, TEnum> : Factory<TPool> 
        where TPool : Poolable<TPool> 
        where TEnum : System.Enum
    {
        /// <summary>
        /// Gets the SmartMultiPool instance from the PoolLocator.
        /// </summary>
        /// <remarks>
        /// If you want to use specific functions of an implementation, don't forget to use "as".
        /// Example: MultiPool as CubePool
        /// </remarks>
        protected virtual SmartMultiPool<TPool, TEnum> MultiPool => PoolLocator.Get<TPool>() as SmartMultiPool<TPool, TEnum>;
        public SerializedDictionary<TEnum, List<TPool>> ActiveItemLists => MultiPool.ActiveObjects;
    }
}
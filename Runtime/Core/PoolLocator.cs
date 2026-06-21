using System;
using System.Collections.Generic;

namespace SmartPools
{
    public static class PoolLocator
    {
        private static readonly Dictionary<object, object> Container = new();

        public static void Add<T>(SmartPool<T> value) where T : Poolable<T>
        {
            if(!Container.ContainsKey(typeof(T)))
                Container.Add(typeof(T), value);
            else
            {
                throw new ArgumentException($"Pool for {typeof(T)} already exists in dictionary.");
            }
        }
        
        /// <summary>
        /// Gets the SmartPool instance from the PoolLocator.
        /// </summary>
        /// <remarks>
        /// If you want to use specific functions of an implementation, don't forget to use "as".
        /// Example: PoolLocator.Get&lt;Shape&gt;() as SmartMultiPool&lt;Shape, ShapeType&gt;
        /// </remarks>
        /// <typeparam name="T">The type of the poolable object.</typeparam>
        /// <returns>The SmartPool instance for the specified type.</returns>
        /// <exception cref="NotImplementedException">Thrown when the pool for the specified type is not available.</exception>
        public static SmartPool<T> Get<T>() where T : Poolable<T>
        {
            try
            {
                return (SmartPool<T>)Container[typeof(T)];
            }
            catch (Exception)
            {
                throw new NotImplementedException($"Pool for {typeof(T)} is not available.");
            }
        }
    }
}
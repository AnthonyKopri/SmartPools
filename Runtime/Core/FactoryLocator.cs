using System;
using System.Collections.Generic;

namespace SmartPools
{
    public class FactoryLocator
    {
        private static readonly Dictionary<object, object> Container = new();

        public static void Add<T>(Factory<T> value) where T : Poolable<T>
        {
            if(!Container.ContainsKey(typeof(T)))
                Container.Add(typeof(T), value);
            else
            {
                throw new ArgumentException($"Factory for {typeof(T)} already exists in dictionary.");
            }
        }
        
        
        /// <summary>
        /// Gets the Factory instance from the FactoryLocator.
        /// </summary>
        /// <remarks>
        /// If you want to use specific functions of an implementation, don't forget to use "as".
        /// Example: FactoryLocator.Get&lt;Shape&gt;() as MultiFactory&lt;Shape&gt;
        /// </remarks>
        /// <typeparam name="T">The type of the poolable object.</typeparam>
        /// <returns>The Factory instance for the specified type.</returns>
        /// <exception cref="NotImplementedException">Thrown when the factory for the specified type is not available.</exception>
        public static Factory<T> Get<T>() where T : Poolable<T>
        {
            try
            {
                return (Factory<T>)Container[typeof(T)];
            }
            catch (Exception)
            {
                throw new NotImplementedException($"Factory for {typeof(T)} is not available.");
            }
        }
    }
}
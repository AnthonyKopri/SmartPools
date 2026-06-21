using System;
using System.Collections.Generic;
using SmartPools.EnumMaps;
using UnityEngine;
using UnityEngine.Pool;

namespace SmartPools
{
    public abstract class SmartMultiPool<TPool, TEnum> : SmartPool<TPool> 
        where TPool : Poolable<TPool> 
        where TEnum : struct, Enum
    {
        [SerializeField] protected EnumMap<TEnum,TPool> prefabDictionary;
        protected override ObjectPool<TPool> Pool { get; set; }
        protected abstract Dictionary<TEnum, ObjectPool<TPool>> MultiPool { get; set; }
        public EnumMap<TEnum, List<TPool>> ActiveObjects;

        private Dictionary<TEnum, int> _creationIndexes = new();
        public override void InitializePool()
        {
            base.InitializePool();
            MultiPool = new Dictionary<TEnum, ObjectPool<TPool>>();
            ActiveObjects = new EnumMap<TEnum, List<TPool>>();
            DefineMultiPool();
        }

        protected virtual void DefineMultiPool()
        {
            //For each enum value
            foreach (TEnum key in Enum.GetValues(typeof(TEnum)))
            {
                var pool = new ObjectPool<TPool>(
                    () => CreateObject(key), 
                    OnTakeFromPool, 
                    (TPool obj) => OnReturnedToPool(obj, key), 
                    OnDestroyPoolObject, 
                    true, initialSize, maxSize);
                
                MultiPool.Add(key, pool);
            }
        }
        
        public TPool Get(TEnum key)
        {
            if(!MultiPool.ContainsKey(key))
                throw new Exception($"Pool with key {key} does not exist");
            
            var obj = MultiPool[key].Get();
            ObjectsInUse.Add(obj);
            if (!ActiveObjects.ContainsKey(key))
                ActiveObjects.Add(key, new List<TPool>());
            ActiveObjects[key].Add(obj);
            return obj;
        }
        
        protected virtual TPool CreateObject(TEnum key)
        {
            if(!prefabDictionary.ContainsKey(key))
                throw new Exception($"Prefab with key {key} does not exist");
            if(!MultiPool.ContainsKey(key))
                throw new Exception($"Pool with key {key} does not exist");
            
            var obj = Instantiate(prefabDictionary[key], transform);
            obj.SetPool(MultiPool[key]);
            
            _creationIndexes.TryAdd(key, 0);
            
            obj.name = $"{key}_{++_creationIndexes[key]}";
            OnObjectCreated?.Invoke(obj);
            return obj;
        }
        
        protected virtual void OnReturnedToPool(TPool obj, TEnum key)
        {
            base.OnReturnedToPool(obj);
            ActiveObjects[key].Remove(obj);
        }

    }
}
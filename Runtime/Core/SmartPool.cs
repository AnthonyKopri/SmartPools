using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Pool;

namespace SmartPools
{
    public abstract class SmartPool<T> : MonoBehaviour where T : Poolable<T>
    {
        public Action<T> OnObjectCreated;
        public Action<T> OnObjectTaken;
        public Action<T> OnObjectReturned;
        public Action OnObjectDestroyed;
        
        [SerializeField] protected T prefab;
        
        [SerializeField] protected int initialSize = 10;
        [SerializeField] protected int maxSize = 100;
        
        protected abstract ObjectPool<T> Pool { get; set; }
        public List<T> ObjectsInUse { get; } = new();
        
        protected uint _creationIndex = 0;

        protected void Awake()
        {
            InitializePool();
        }

        public virtual void InitializePool()
        {
            PoolLocator.Add(this);
            DefinePool();
        }
        
        protected virtual void DefinePool()
        {
            Pool = new ObjectPool<T>(
                () => CreateObject(), 
                OnTakeFromPool, 
                OnReturnedToPool, 
                OnDestroyPoolObject, 
                true, initialSize, maxSize);
        }
        
        public T Get()
        {
            var obj = Pool.Get();
            ObjectsInUse.Add(obj);
            return obj;
        }
        
        protected virtual T CreateObject()
        {
            var obj = Instantiate(prefab, transform);
            obj.SetPool(Pool);
            obj.name = $"{typeof(T).Name}_{++_creationIndex}";
            OnObjectCreated?.Invoke(obj);
            return obj;
        }
        
        protected virtual void OnTakeFromPool(T obj)
        {
            obj.gameObject.SetActive(true);
            obj.OnTakeFromPool();
            OnObjectTaken?.Invoke(obj);
        }
        
        protected virtual void OnReturnedToPool(T obj)
        {
            ObjectsInUse.Remove(obj);
            obj.gameObject.SetActive(false);
            OnObjectReturned?.Invoke(obj);
        }
        
        protected virtual void OnDestroyPoolObject(T obj)
        {
            Destroy(obj.gameObject);
            OnObjectDestroyed?.Invoke();
        }
        
        public void ReleaseAll()
        {
            var objects = new List<T>(ObjectsInUse);
            foreach (var obj in objects)
            {
                if(obj == null) continue;
                obj.Release();
            }
            
            ObjectsInUse.Clear();
        }
    }
}
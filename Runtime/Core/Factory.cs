using System.Collections.Generic;
using UnityEngine;

namespace SmartPools
{
    public abstract class Factory<T> : MonoBehaviour where T : Poolable<T> 
    {
        protected SmartPool<T> Pool => PoolLocator.Get<T>();
        public List<T> ItemsInUse => Pool.ObjectsInUse;
        
        public virtual void Clean()
        {
            Pool.ReleaseAll();
        }
    }
}
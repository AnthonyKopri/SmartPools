using UnityEngine;
using UnityEngine.Pool;

namespace SmartPools
{
    public abstract class Poolable<TPool> : MonoBehaviour where TPool : class
    {
        protected ObjectPool<TPool> Pool;
        private bool _isReleased;
        
        public void SetPool(ObjectPool<TPool> objectPool)
        {
            Pool = objectPool;
        }

        public virtual void OnTakeFromPool()
        {
            _isReleased = false;
        }
        
        public virtual void Release()
        {
            if (_isReleased)
                return;
            
            _isReleased = true;
            Pool.Release(this as TPool);
        }
    }
}
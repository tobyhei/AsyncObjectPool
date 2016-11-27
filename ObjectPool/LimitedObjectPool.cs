using System;
using System.Collections.Generic;
using System.Threading;
using ObjectPool.Misc;

namespace ObjectPool
{
    public class LimitedObjectPool<T>
    {
        private readonly Queue<T> _queue = new Queue<T>();
        private readonly Func<Optional<T>> _creatorFunc;

        public LimitedObjectPool(Func<Optional<T>> creatorFunc)
        {
            _creatorFunc = creatorFunc;
        }

        public Pooled<T> Get()
        {
            lock (_queue)
            {   
                if (_queue.Count > 0)
                {
                    return CreatePooled(_queue.Dequeue());
                }

                var newResource = _creatorFunc();
                if (newResource.HasValue)
                {
                    return CreatePooled(newResource.Value);
                }

                while (_queue.Count == 0)
                {
                    Monitor.Wait(_queue);
                }

                return CreatePooled(_queue.Dequeue());
            }
        }

        private Pooled<T> CreatePooled(T resource) =>
            new Pooled<T>(() => ReturnResource(resource), resource);

        private void ReturnResource(T resource)
        {
            lock (_queue)
            {
                _queue.Enqueue(resource);
                Monitor.Pulse(_queue);
            }
        }
    }
}

using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Nito.AsyncEx;
using ObjectPool;
using ObjectPool.Misc;

namespace ObjectPoolAsync
{
    /// <summary>
    /// Provides an object pool for resources that cannot be infintely created
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public class LimitedObjectPoolAsync<T>
    {
        private readonly Func<Optional<T>> _creatorFunc;
        private readonly Queue<T> _queue;
        private readonly AsyncLock _lock;
        private readonly AsyncConditionVariable _conditionVariable;

        public LimitedObjectPoolAsync(Func<Optional<T>> creatorFunc)
        {
            _creatorFunc = creatorFunc;
            _queue = new Queue<T>();
            _lock = new AsyncLock();
            _conditionVariable = new AsyncConditionVariable(_lock);
        }

        /// <summary>
        /// Trys to get a resource from the object pool
        /// Fails if no more can be created and none have yet been returned to the pool
        /// </summary>
        /// <returns></returns>
        public Optional<Pooled<T>> TryGet()
        {
            using (_lock.Lock())
            {
                return _queue.Count > 0 ?
                    Optional<Pooled<T>>.Some(CreatePooled(_queue.Dequeue())) :
                    _creatorFunc().Transform(CreatePooled);
            }
        }

        /// <summary>
        /// Gets a resource from the object pool
        /// if no resources are available then waits asynchronously for one to be returned
        /// </summary>
        /// <returns></returns>
        public async Task<Pooled<T>> GetAsync(CancellationToken cancellationToken = default(CancellationToken))
        {
            using (await _lock.LockAsync(cancellationToken))
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
                    await _conditionVariable.WaitAsync(cancellationToken);
                }

                return CreatePooled(_queue.Dequeue());
            }
        }

        private Pooled<T> CreatePooled(T resource)
            => new Pooled<T>(() => ReturnResource(resource), resource);

        private void ReturnResource(T resource)
        {
            using (_lock.Lock())
            {
                _queue.Enqueue(resource);
                _conditionVariable.Notify();
            }
        }
    }
}

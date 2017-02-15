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
    /// Provides an object pool for resources that cannot be infinitely created
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public class LimitedObjectPoolAsync<T>
    {
        private readonly Func<Optional<T>> _creatorFunc;
        private readonly Queue<T> _queue;
        private readonly AsyncMonitor _monitor;

        public LimitedObjectPoolAsync(Func<Optional<T>> creatorFunc)
        {
            _creatorFunc = creatorFunc;
            _queue = new Queue<T>();
            _monitor = new AsyncMonitor();
        }

        /// <summary>
        /// Attempts to get a resource from the object pool
        /// Fails if no more can be created and none have yet been returned to the pool
        /// </summary>
        /// <returns></returns>
        public Optional<Pooled<T>> TryGet()
        {
            using (_monitor.Enter())
            {
                return _queue.Count > 0 ?
                    Optional<Pooled<T>>.Some(CreatePooled(_queue.Dequeue())) :
                    _creatorFunc().Transform(CreatePooled);
            }
        }

        /// <summary>
        /// Attempts to get a resource from the object pool
        /// if no resources are available then waits asynchronously for one to be returned
        /// </summary>
        /// <returns></returns>
        public async Task<Optional<Pooled<T>>> TryGetAsync(CancellationToken cancellationToken = default(CancellationToken))
        {
            using (await _monitor.EnterAsync().ConfigureAwait(false))
            {
                if (cancellationToken.IsCancellationRequested)
                {
                    return Optional<Pooled<T>>.None();
                }

                if (_queue.Count > 0)
                {
                    return Optional<Pooled<T>>.Some(CreatePooled(_queue.Dequeue()));
                }

                var newResource = _creatorFunc();
                if (newResource.HasValue)
                {
                    return Optional<Pooled<T>>.Some(CreatePooled(newResource.Value));
                }

                while (_queue.Count == 0)
                {
                    await _monitor.WaitAsync(cancellationToken).ConfigureAwait(false);

                    if (cancellationToken.IsCancellationRequested)
                    {
                        return Optional<Pooled<T>>.None();
                    }
                }

                return Optional<Pooled<T>>.Some(CreatePooled(_queue.Dequeue()));
            }
        }

        private Pooled<T> CreatePooled(T resource)
            => new Pooled<T>(() => ReturnResource(resource), resource);

        private void ReturnResource(T resource)
        {
            using (_monitor.Enter())
            {
                _queue.Enqueue(resource);
                _monitor.Pulse();
            }
        }
    }
}

using System;

namespace ObjectPool
{
    public class Pooled<T> : IDisposable
    {
        private readonly Action _cleanupFunc;
        private bool _isDisposed = false;

        public T Resource { get; }

        public Pooled(Action cleanupFunc, T resource)
        {
            _cleanupFunc = cleanupFunc;
            Resource = resource;
        }

        ~Pooled() 
        {
            if (!_isDisposed)
            {
                Dispose();
            }
        }

        public void Dispose()
        {
            _cleanupFunc();
            _isDisposed = true;
        }
    }
}
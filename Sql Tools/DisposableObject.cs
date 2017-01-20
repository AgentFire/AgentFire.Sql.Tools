using System;

namespace AgentFire.Sql.Tools
{
    public abstract class DisposableObject : IDisposable
    {
        protected bool IsDisposed { get; private set; } = false;

        private readonly object _disposingLock = new object();
        private bool _isDisposing = false;

        public void Dispose()
        {
            if (IsDisposed || _isDisposing)
            {
                return;
            }

            lock (_disposingLock)
            {
                if (IsDisposed || _isDisposing)
                {
                    return;
                }

                _isDisposing = true;
                DisposeInternal();
                IsDisposed = true;
            }
        }
        protected abstract void DisposeInternal();

        protected void EnsureNotDisposed()
        {
            if (IsDisposed)
            {
                throw new ObjectDisposedException(nameof(DisposableObject));
            }
        }
    }
}

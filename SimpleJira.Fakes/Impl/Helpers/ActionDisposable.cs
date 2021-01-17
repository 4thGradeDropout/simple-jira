using System;

namespace SimpleJira.Fakes.Impl.Helpers
{
    internal class ActionDisposable : IDisposable
    {
        private readonly Action onDispose;

        public ActionDisposable(Action onDispose)
        {
            this.onDispose = onDispose;
        }

        protected virtual void Dispose(bool disposing)
        {
            if (disposing)
                onDispose();
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }
    }
}
using System;
using System.Threading;

namespace SimpleJira.Fakes.Impl.Helpers
{
    internal static class MutexLock
    {
        public static IDisposable Lock(string name, TimeSpan timeout)
        {
            var mutex = new Mutex(false, name);
            bool acquired;
            try
            {
                acquired = mutex.WaitOne(timeout);
                if (!acquired)
                {
                    mutex.Dispose();
                    throw new MutexLockingException(name, timeout);
                }
            }
            catch (AbandonedMutexException)
            {
                mutex.ReleaseMutex();
                mutex.Dispose();
                acquired = false;
            }

            return new ActionDisposable(() =>
            {
                if (acquired)
                    mutex.ReleaseMutex();

                mutex.Close();
            });
        }
    }
}
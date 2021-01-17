using System;

namespace SimpleJira.Fakes.Impl.Helpers
{
    internal class MutexLockingException : Exception
    {
        public MutexLockingException(string name, TimeSpan span)
            : base($"mutex [{name}] was not acquired on [{span}]")
        {
        }
    }
}
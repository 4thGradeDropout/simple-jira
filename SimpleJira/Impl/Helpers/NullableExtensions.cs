using System;

namespace SimpleJira.Impl.Helpers
{
    internal static class NullableExtensions
    {
        public static T GetValueOrDie<T>(this T? value) where T : struct
        {
            if (value.HasValue)
                return value.Value;
            throw new InvalidOperationException($"Value of type [{typeof(T).Name}] is null");
        }
    }
}
using System;
using System.Collections.Concurrent;
using System.Linq;
using System.Reflection;
using SimpleJira.Interface.Metadata;

namespace SimpleJira.Impl
{
    internal static class JiraMetadataProviderExtensions
    {
        private static readonly ConcurrentDictionary<(IJiraMetadataProvider key, PropertyInfo property), Func<IJiraFieldMetadata>>
            jiraProperties =
                new ConcurrentDictionary<(IJiraMetadataProvider key, PropertyInfo field), Func<IJiraFieldMetadata>>();

        public static IJiraFieldMetadata GetFieldMetadata(this IJiraMetadataProvider provider,
            PropertyInfo property)
        {
            var key = (key: provider, property);
            return jiraProperties.GetOrAdd(key, k =>
            {
                var properties = provider.Issues.SelectMany(x => x.Fields)
                    .Where(x => x.Property == k.property)
                    .Take(2)
                    .ToArray();
                if (properties.Length == 0)
                    return () => throw new InvalidOperationException(
                        $"there are no properties with name [{k.property.DeclaringType}.{k.property.Name}]");
                if (properties.Length > 1)
                    return () => throw new InvalidOperationException(
                        $"more than one property with name [{k.property.DeclaringType}.{k.property.Name}] are detected");
                return () => properties[0];
            })();
        }
    }
}
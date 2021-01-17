using System;
using System.Collections.Concurrent;
using System.Linq;

namespace SimpleJira.Interface.Metadata
{
    public static class JiraMetadataProviderExtensions
    {
        private static readonly ConcurrentDictionary<(IJiraMetadataProvider key, string field), Func<Type>> fieldTypes =
            new ConcurrentDictionary<(IJiraMetadataProvider key, string field), Func<Type>>();

        public static Type GetFieldType(this IJiraMetadataProvider provider, string fieldName)
        {
            var key = (key: provider, field: fieldName);
            return fieldTypes.GetOrAdd(key, k =>
            {
                var types = provider.Issues.SelectMany(x => x.Fields)
                    .Where(x => string.Equals(x.FieldName, k.field, StringComparison.InvariantCultureIgnoreCase))
                    .Select(x => x.Property.PropertyType)
                    .Distinct()
                    .Take(2)
                    .ToArray();
                if (types.Length == 0)
                    return () => throw new JiraException($"there are no fields with name '{k.field}'");
                if (types.Length > 1)
                    return () => throw new JiraException($"more than one field with name '{k.field}' are detected");
                return () => types[0];
            })();
        }
    }
}
using System;
using System.Collections.Concurrent;
using System.Linq.Expressions;
using System.Reflection;
using SimpleJira.Impl.Helpers;
using SimpleJira.Interface.Issue;

namespace SimpleJira.Impl.Queryable
{
    internal static class ProjectionMapperFactory
    {
        private static readonly ParameterExpression xParameter = Expression.Parameter(typeof(object[]));

        private static readonly ConcurrentDictionary<string, Func<object[], object>> compiledProperties =
            new ConcurrentDictionary<string, Func<object[], object>>(StringComparer.InvariantCultureIgnoreCase);

        public static Func<JiraIssue, object> GetMapper(Projection projection)
        {
            var fieldsExtractor = CreateFieldsExtractor(projection);
            var ctorArguments = CreateArgumentsExtractor(projection.ctorProperties);
            var arguments = CreateArgumentsExtractor(projection.properties);
            var instanceFactory = CreateInstanceFactory(projection);
            return queryResultRow =>
            {
                var fieldValues = fieldsExtractor(queryResultRow);
                return instanceFactory(ctorArguments(queryResultRow, fieldValues),
                    arguments(queryResultRow, fieldValues));
            };
        }

        private static Func<JiraIssue, object[]> CreateFieldsExtractor(Projection projection)
        {
            var fieldValues = new object[projection.fields.Length];
            return delegate(JiraIssue queryResultRow)
            {
                for (var i = 0; i < fieldValues.Length; i++)
                {
                    var field = projection.fields[i];
                    fieldValues[i] = field.GetValue(queryResultRow);
                }

                return fieldValues;
            };
        }

        private static Func<JiraIssue, object[], object[]> CreateArgumentsExtractor(SelectedProperty[] properties)
        {
            if (properties == null || properties.Length == 0)
                return (queryResultRow, fieldValues) => ReflectionHelpers.noParameters;

            var argumentsCount = 0;
            var parameterizer = new ParameterizingExpressionVisitor();
            foreach (var property in properties)
            {
                if (property.isReference)
                    continue;
                var expression = property.expression;
                if (!property.needLocalEval)
                    continue;
                if (property.items.Length > argumentsCount)
                    argumentsCount = property.items.Length;
                var xBody = parameterizer.Parameterize(expression, xParameter);
                var key = xBody.ToString();
                if (!compiledProperties.TryGetValue(key, out var compiledExpression))
                {
                    var xConvertBody = Expression.Convert(xBody, typeof(object));
                    var xLambda = Expression.Lambda<Func<object[], object>>(xConvertBody, xParameter);
                    compiledExpression = xLambda.Compile();
                    compiledProperties.TryAdd(key, compiledExpression);
                }

                property.compiledExpression = compiledExpression;
            }

            var propertyValues = new object[properties.Length];
            var propArguments = argumentsCount > 0 ? new object[argumentsCount] : null;
            return delegate(JiraIssue queryResultRow, object[] fieldValues)
            {
                for (var i = 0; i < properties.Length; i++)
                {
                    var property = properties[i];
                    if (property.isReference)
                        propertyValues[i] = queryResultRow.Reference();
                    else if (propArguments != null && property.needLocalEval)
                    {
                        for (var j = 0; j < property.items.Length; j++)
                            propArguments[j] = property.items[j].GetValue(fieldValues);
                        propertyValues[i] = properties[i].compiledExpression(propArguments);
                    }
                    else
                        propertyValues[i] = property.items[0].GetValue(fieldValues);
                }

                return propertyValues;
            };
        }

        private static Func<object[], object[], object> CreateInstanceFactory(Projection projection)
        {
            if (projection.ctor == null)
                return (_, a) => a[0];
            var compiledCtorDelegate = ReflectionHelpers.GetCompiledDelegate(projection.ctor);
            if (projection.initMembers == null)
                return (ctorArguments, _) => compiledCtorDelegate(null, ctorArguments);
            var memberAccessors = new PropertyAccessor[projection.initMembers.Length];
            for (var i = 0; i < memberAccessors.Length; i++)
            {
                var property = (PropertyInfo) projection.initMembers[i];
                memberAccessors[i] = PropertyAccessor.Get(property);
            }

            return delegate(object[] ctorArguments, object[] arguments)
            {
                var result = compiledCtorDelegate(null, ctorArguments);
                for (var i = 0; i < memberAccessors.Length; i++)
                    memberAccessors[i].Set(result, arguments[i]);
                return result;
            };
        }
    }
}
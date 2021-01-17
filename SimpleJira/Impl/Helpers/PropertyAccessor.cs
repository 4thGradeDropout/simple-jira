using System;
using System.Collections.Concurrent;
using System.Linq.Expressions;
using System.Reflection;

namespace SimpleJira.Impl.Helpers
{
    internal class PropertyAccessor
    {
        private readonly PropertyInfo propertyInfo;
        private readonly Func<object, object> getter;
        private readonly Action<object, object> setter;

        private static readonly ConcurrentDictionary<PropertyInfo, PropertyAccessor> cache =
            new ConcurrentDictionary<PropertyInfo, PropertyAccessor>();

        private PropertyAccessor(PropertyInfo propertyInfo, Func<object, object> getter,
            Action<object, object> setter)
        {
            this.propertyInfo = propertyInfo;
            this.getter = getter;
            this.setter = setter;
        }

        public object Get(object obj)
        {
            return getter(obj);
        }

        public void Set(object obj, object value)
        {
            if (setter == null)
                throw new InvalidOperationException(
                    $"the property [{propertyInfo.Name}] of the type [{propertyInfo.DeclaringType.Name}] doesn't have a setter");
            setter(obj, value);
        }

        public static PropertyAccessor Get(PropertyInfo propertyInfo)
        {
            return cache.GetOrAdd(propertyInfo, p =>
            {
                var objectType = p.DeclaringType;
                var valueType = p.PropertyType;
                var xObjectParameter = Expression.Parameter(typeof(object), "obj");
                var xValueParameter = Expression.Parameter(typeof(object), "value");

                var getter = Expression.Lambda<Func<object, object>>(
                    Expression.Convert(Expression.Property(Expression.Convert(xObjectParameter, objectType), p),
                        typeof(object)),
                    xObjectParameter
                ).Compile();

                Action<object, object> setter = null;
                if (p.SetMethod != null)
                    setter = Expression.Lambda<Action<object, object>>(
                        Expression.Call(Expression.Convert(xObjectParameter, objectType), p.SetMethod,
                            Expression.Convert(xValueParameter, valueType)),
                        xObjectParameter, xValueParameter
                    ).Compile();

                return new PropertyAccessor(p, getter, setter);
            });
        }
    }
}
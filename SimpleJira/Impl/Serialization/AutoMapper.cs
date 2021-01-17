using System;
using System.Collections.Concurrent;
using System.Reflection;
using SimpleJira.Impl.Helpers;

namespace SimpleJira.Impl.Serialization
{
    internal class AutoMapper
    {
        private readonly Func<object, object> mapper;
        private static readonly Assembly assembly = typeof(AutoMapper).Assembly;

        private static readonly ConcurrentDictionary<(Type from, Type to), AutoMapper>
            cache = new ConcurrentDictionary<(Type from, Type to), AutoMapper>();

        private AutoMapper(Func<object, object> mapper)
        {
            this.mapper = mapper;
        }

        public object Map(object obj)
        {
            return obj == null ? null : mapper(obj);
        }

        public static AutoMapper Create<TFrom, TTo>()
        {
            return Create(typeof(TFrom), typeof(TTo));
        }

        public static AutoMapper Create(Type from, Type to)
        {
            return cache.GetOrAdd((@from, to), k =>
            {
                if (k.from == k.to)
                    return new AutoMapper(o => o);
                if (k.from.IsArray && k.to.IsArray)
                {
                    var elementType = k.from.GetElementType();
                    var dtoElementType = k.to.GetElementType();
                    return new AutoMapper(o =>
                    {
                        var source = (Array) o;
                        if (o == null)
                            return null;
                        var converter = Create(elementType, dtoElementType);
                        var target = Array.CreateInstance(dtoElementType, source.Length);
                        for (var i = 0; i < source.Length; ++i)
                            target.SetValue(converter.Map(source.GetValue(i)), i);
                        return target;
                    });
                }

                if (k.from.Assembly == assembly && k.to.Assembly == assembly)
                {
                    var objectProperties = k.from.GetProperties(BindingFlags.Instance | BindingFlags.Public);
                    Array.Sort(objectProperties,
                        (p1, p2) => string.Compare(p1.Name, p2.Name, StringComparison.InvariantCulture));
                    var dtoObjectProperties =
                        k.to.GetProperties(BindingFlags.Instance | BindingFlags.Public);
                    Array.Sort(dtoObjectProperties,
                        (p1, p2) => string.Compare(p1.Name, p2.Name, StringComparison.InvariantCulture));
                    if (objectProperties.Length != dtoObjectProperties.Length)
                        throw new InvalidOperationException(
                            $"can't create mapper from '{k.from.Name}' to {k.to.Name}");
                    var mappers = new Action<object, object>[objectProperties.Length];
                    for (var i = 0; i < objectProperties.Length; ++i)
                    {
                        var objectProperty = objectProperties[i];
                        var dtoObjectProperty = dtoObjectProperties[i];
                        if (objectProperty.Name != dtoObjectProperty.Name)
                            throw new InvalidOperationException(
                                $"can't create mapper from '{k.from.Name}' to {k.to.Name}");

                        var objectPropertyAccessor = PropertyAccessor.Get(objectProperty);
                        var dtoObjectPropertyAccessor = PropertyAccessor.Get(dtoObjectProperty);
                        mappers[i] = (source, target) =>
                        {
                            var doMap = Create(objectProperty.PropertyType, dtoObjectProperty.PropertyType);
                            var sourceProperty = objectPropertyAccessor.Get(source);
                            var targetProperty = doMap.Map(sourceProperty);
                            dtoObjectPropertyAccessor.Set(target, targetProperty);
                        };
                    }

                    return new AutoMapper(o =>
                    {
                        var r = k.to.New();
                        foreach (var mapper1 in mappers)
                            mapper1(o, r);
                        return r;
                    });
                }

                throw new InvalidOperationException(
                    $"can't create mapper from '{k.from.Name}' to '{k.to.Name}'");
            });
        }
    }
}
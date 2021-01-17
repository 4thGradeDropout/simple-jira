using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;

namespace SimpleJira.Impl.Helpers
{
    internal static class ReflectionHelpers
    {
        public static readonly Type[] emptyTypes = new Type[0];
        public static readonly object[] noParameters = new object[0];

        public static object GetDefaultValue(this Type type)
        {
            if (type.IsValueType)
            {
                if (Nullable.GetUnderlyingType(type) != null)
                    return null;
                if (type == typeof(int))
                    return default(int);
                if (type == typeof(long))
                    return default(long);
                if (type == typeof(decimal))
                    return default(decimal);
                if (type == typeof(float))
                    return default(float);
                if (type == typeof(double))
                    return default(double);
                if (type == typeof(DateTime))
                    return default(DateTime);
                return type.New();
            }

            return null;
        }

        public static object New(this Type type)
        {
            var constructorInfo = type.GetConstructor(emptyTypes);
            if (constructorInfo == null)
                throw new InvalidOperationException(
                    $"default constructor of the type [{type}] is absent");
            return GetCompiledDelegate(constructorInfo)(null, noParameters);
        }

        public static string FormatName(this Type type)
        {
            if (typeNames.TryGetValue(type, out var result))
                return result;
            if (type.IsArray)
                return type.GetElementType().FormatName() + "[]";
            if (type.IsDelegate() && type.IsNested)
                return type.DeclaringType.FormatName() + "." + type.Name;

            if (!type.IsNested || !type.DeclaringType.IsGenericType || type.IsGenericParameter)
                return FormatGenericType(type, type.GetGenericArguments());

            var declaringHierarchy = DeclaringHierarchy(type)
                .TakeWhile(t => t.IsGenericType)
                .Reverse();

            var knownGenericArguments = type.GetGenericTypeDefinition().GetGenericArguments()
                .Zip(type.GetGenericArguments(), (definition, closed) => new {definition, closed})
                .ToDictionary(x => x.definition.GenericParameterPosition, x => x.closed);

            var hierarchyNames = new List<string>();

            foreach (var t in declaringHierarchy)
            {
                var tArguments = t.GetGenericTypeDefinition()
                    .GetGenericArguments()
                    .Where(x => knownGenericArguments.ContainsKey(x.GenericParameterPosition))
                    .ToArray();

                hierarchyNames.Add(FormatGenericType(t,
                    tArguments.Select(x => knownGenericArguments[x.GenericParameterPosition]).ToArray()));

                foreach (var tArgument in tArguments)
                    knownGenericArguments.Remove(tArgument.GenericParameterPosition);
            }

            return string.Join(".", hierarchyNames.ToArray());
        }

        private static IEnumerable<Type> DeclaringHierarchy(Type type)
        {
            yield return type;
            while (type.DeclaringType != null)
            {
                yield return type.DeclaringType;
                type = type.DeclaringType;
            }
        }

        private static bool IsDelegate(this Type type)
        {
            return type.BaseType == typeof(MulticastDelegate);
        }

        private static string FormatGenericType(Type type, Type[] arguments)
        {
            var genericMarkerIndex = type.Name.IndexOf("`", StringComparison.InvariantCulture);
            return genericMarkerIndex > 0
                ? $"{type.Name.Substring(0, genericMarkerIndex)}<{string.Join(",", arguments.Select(FormatName))}>"
                : type.Name;
        }

        private static readonly IDictionary<Type, string> typeNames = new Dictionary<Type, string>
        {
            {typeof(object), "object"},
            {typeof(byte), "byte"},
            {typeof(short), "short"},
            {typeof(ushort), "ushort"},
            {typeof(int), "int"},
            {typeof(uint), "uint"},
            {typeof(long), "long"},
            {typeof(ulong), "ulong"},
            {typeof(double), "double"},
            {typeof(float), "float"},
            {typeof(string), "string"},
            {typeof(bool), "bool"}
        };

        private static readonly ConcurrentDictionary<MethodBase, Func<object, object[], object>> compiledMethods =
            new ConcurrentDictionary<MethodBase, Func<object, object[], object>>();

        private static readonly Func<MethodBase, Func<object, object[], object>> compileMethodDelegate =
            EmitCallOf;

        public static Func<object, object[], object> GetCompiledDelegate(MethodBase targetMethod)
        {
            return compiledMethods.GetOrAdd(targetMethod, compileMethodDelegate);
        }

        private static Func<object, object[], object> EmitCallOf(MethodBase targetMethod)
        {
            var parameterInfos = targetMethod.GetParameters();
            var xParameterObj = Expression.Parameter(typeof(object), "obj");
            var xParameterPars = Expression.Parameter(typeof(object[]), "parameters");
            var xParameters = new Expression[parameterInfos.Length];
            for (var i = 0; i < xParameters.Length; i++)
                xParameters[i] = Expression.Convert(
                    Expression.ArrayIndex(xParameterPars, Expression.Constant(i)),
                    parameterInfos[i].ParameterType);

            bool isVoid = false;

            Expression xBody;
            if (targetMethod.IsConstructor)
            {
                var constructorInfo = (ConstructorInfo) targetMethod;
                xBody = Expression.New(constructorInfo, (IEnumerable<Expression>) xParameters);
            }
            else
            {
                var methodInfo = (MethodInfo) targetMethod;
                var objectType = methodInfo.DeclaringType;
                Expression xInstance = methodInfo.IsStatic ? null : Expression.Convert(xParameterObj, objectType);
                xBody = Expression.Call(xInstance, methodInfo, (IEnumerable<Expression>) xParameters);
                isVoid = methodInfo.ReturnType == typeof(void);
            }

            if (isVoid)
            {
                var action = Expression.Lambda<Action<object, object[]>>(xBody,
                        xParameterObj, xParameterPars)
                    .Compile();
                return (obj, parameters) =>
                {
                    action(obj, parameters);
                    return null;
                };
            }

            return Expression.Lambda<Func<object, object[], object>>(
                    Expression.Convert(xBody, typeof(object)),
                    xParameterObj, xParameterPars)
                .Compile();
        }
    }
}
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace SimpleJira.Impl.Utilities
{
    internal static class EnumAttributesCache<TAttribute>
        where TAttribute : Attribute
    {
        private static readonly ConcurrentDictionary<Type, IDictionary<string, TAttribute>> enumToItems =
            new ConcurrentDictionary<Type, IDictionary<string, TAttribute>>();

        public static TAttribute GetAttribute<TEnum>(TEnum enumItem)
            where TEnum : struct
        {
            return GetAttributeUnsafe(enumItem);
        }

        public static TAttribute GetAttributeUnsafe(object enumItem)
        {
            TAttribute result;
            if (!GetAllAttributes(enumItem.GetType())
                .TryGetValue(enumItem.ToString(), out result))
                throw new ArgumentOutOfRangeException(
                    nameof(enumItem),
                    $"enum [{enumItem.GetType().FullName}] has no [{typeof(TAttribute).Name}] for [{enumItem}]");
            return result;
        }

        public static IDictionary<string, TAttribute> GetAllAttributes(Type enumType)
        {
            return enumToItems.GetOrAdd(enumType, GetEnumItems);
        }

        private static IDictionary<string, TAttribute> GetEnumItems(Type enumType)
        {
            return enumType
                .GetFields()
                .Where(item => !item.IsSpecialName)
                .Select(item => new {item.Name, Attr = GetEnumItemAttribute(item)})
                .Where(x => x.Attr != null)
                .ToDictionary(x => x.Name, x => x.Attr);
        }

        private static TAttribute GetEnumItemAttribute(FieldInfo enumItem)
        {
            return (TAttribute) enumItem
                .GetCustomAttributes(typeof(TAttribute), false)
                .FirstOrDefault();
        }
    }
}
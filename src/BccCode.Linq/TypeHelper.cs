﻿using System.Numerics;

namespace BccCode.Linq;

internal static class TypeHelper
{
    public static Type? GetElementType(Type? seqType)
    {
        Type? ienum = FindIEnumerable(seqType);
        if (ienum == null) return seqType;
        return ienum.GetGenericArguments()[0];
    }

    private static Type? FindIEnumerable(Type? seqType)
    {
        if (seqType == null || seqType == typeof(string))
            return null;

        if (seqType.IsArray)
            return typeof(IEnumerable<>).MakeGenericType(seqType.GetElementType());

        if (seqType.IsGenericType)
        {
            foreach (Type arg in seqType.GetGenericArguments())
            {
                Type ienum = typeof(IEnumerable<>).MakeGenericType(arg);
                if (ienum.IsAssignableFrom(seqType))
                {
                    return ienum;
                }
            }
        }

        Type[] ifaces = seqType.GetInterfaces();
        if (ifaces != null && ifaces.Length > 0)
        {
            foreach (Type iface in ifaces)
            {
                Type ienum = FindIEnumerable(iface);
                if (ienum != null) return ienum;
            }
        }

        if (seqType.BaseType != null && seqType.BaseType != typeof(object))
        {
            return FindIEnumerable(seqType.BaseType);
        }

        return null;
    }

    public static bool IsAssignableToGenericType(this Type givenType, Type genericType)
    {
        if (givenType == null) throw new ArgumentNullException(nameof(givenType));
        if (genericType == null) throw new ArgumentNullException(nameof(genericType));

        if (givenType.IsGenericType && givenType.GetGenericTypeDefinition() == genericType)
            return true;

        var interfaceTypes = givenType.GetInterfaces();
        if (interfaceTypes.Any(it => it.IsGenericType && it.GetGenericTypeDefinition() == genericType))
        {
            return true;
        }

        Type? baseType = givenType.BaseType;
        if (baseType == null)
            return false;

        return IsAssignableToGenericType(baseType, genericType);
    }

#if NET7_0_OR_GREATER
#else
    private static readonly HashSet<Type> NumericTypes = new()
    {
        typeof(int),  typeof(double),  typeof(decimal),
        typeof(long), typeof(short),   typeof(sbyte),
        typeof(byte), typeof(ulong),   typeof(ushort),
        typeof(uint), typeof(float),   typeof(BigInteger)
    };
#endif

    public static bool IsNumberType(Type type)
    {
#if NET7_0_OR_GREATER
        var numType = typeof(INumber<>);
        var result = type.GetInterfaces().Any(i => i.IsGenericType && (i.GetGenericTypeDefinition() == numType));
        return result;
#else
        return NumericTypes.Contains(type);
#endif
    }
}

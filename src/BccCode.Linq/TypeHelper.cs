using System.Diagnostics.CodeAnalysis;
using System.Numerics;

namespace BccCode.Linq;

internal static class TypeHelper
{
#if NET7_0_OR_GREATER
    [UnconditionalSuppressMessage("AOT", "IL3050", Justification = "Only used by ApiQueryProvider.CreateQuery(Expression), which is dynamic by design and only reachable through GetQueryable<T>, which is annotated with RequiresDynamicCode. The types searched are the interfaces of the query's own types.")]
    [UnconditionalSuppressMessage("Trimming", "IL2070", Justification = "Only used by ApiQueryProvider.CreateQuery(Expression), which is dynamic by design and only reachable through GetQueryable<T>, which is annotated with RequiresDynamicCode. The types searched are the interfaces of the query's own types.")]
#endif
    public static Type? GetElementType(Type? seqType)
    {
        Type? ienum = FindIEnumerable(seqType);
        if (ienum == null) return seqType;
        return ienum.GetGenericArguments()[0];
    }

#if NET7_0_OR_GREATER
    [UnconditionalSuppressMessage("AOT", "IL3050", Justification = "Only used by ApiQueryProvider.CreateQuery(Expression), which is dynamic by design and only reachable through GetQueryable<T>, which is annotated with RequiresDynamicCode. The types searched are the interfaces of the query's own types.")]
    [UnconditionalSuppressMessage("Trimming", "IL2070", Justification = "Only used by ApiQueryProvider.CreateQuery(Expression), which is dynamic by design and only reachable through GetQueryable<T>, which is annotated with RequiresDynamicCode. The types searched are the interfaces of the query's own types.")]
#endif
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

    private static readonly HashSet<Type> NumericTypes = new()
    {
        typeof(int),  typeof(double),  typeof(decimal),
        typeof(long), typeof(short),   typeof(sbyte),
        typeof(byte), typeof(ulong),   typeof(ushort),
        typeof(uint), typeof(float),   typeof(BigInteger),
#if NET5_0_OR_GREATER
        typeof(Half), typeof(nint),    typeof(nuint),
#endif
#if NET7_0_OR_GREATER
        typeof(Int128), typeof(UInt128),
#endif
    };

    public static bool IsNumberType(Type type)
    {
        return NumericTypes.Contains(type);
    }
}

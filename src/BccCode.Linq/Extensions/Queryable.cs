using System.Reflection;

namespace System.Linq.Internal;

/// <summary>
/// Internal. Holds reflection helper for extension methods of class <see cref="System.Linq.Queryable"/>.
/// </summary>
internal static class Queryable
{
    internal static readonly MethodInfo OrderByMethodInfo
        = typeof(System.Linq.Queryable)
            .GetTypeInfo().GetDeclaredMethods(nameof(System.Linq.Queryable.OrderBy))
            .Single(
                mi => mi.GetGenericArguments().Length == 2
                      && mi.GetParameters().Length == 2);
    
    internal static readonly MethodInfo OrderByDescendingMethodInfo
        = typeof(System.Linq.Queryable)
            .GetTypeInfo().GetDeclaredMethods(nameof(System.Linq.Queryable.OrderByDescending))
            .Single(
                mi => mi.GetGenericArguments().Length == 2
                      && mi.GetParameters().Length == 2);
    
    internal static readonly MethodInfo ThenByMethodInfo
        = typeof(System.Linq.Queryable)
            .GetTypeInfo().GetDeclaredMethods(nameof(System.Linq.Queryable.ThenBy))
            .Single(
                mi => mi.GetGenericArguments().Length == 2
                      && mi.GetParameters().Length == 2);
    
    internal static readonly MethodInfo ThenByDescendingMethodInfo
        = typeof(System.Linq.Queryable)
            .GetTypeInfo().GetDeclaredMethods(nameof(System.Linq.Queryable.ThenByDescending))
            .Single(
                mi => mi.GetGenericArguments().Length == 2
                      && mi.GetParameters().Length == 2);
}

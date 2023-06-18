using System.Reflection;

namespace System.Linq.Internal;

/// <summary>
/// Internal. Holds reflection helper for extension methods of class <see cref="System.Linq.Enumerable"/>.
/// </summary>
internal static class Enumerable
{
    internal static readonly MethodInfo FirstMethodInfo
        = typeof(System.Linq.Enumerable)
            .GetTypeInfo().GetDeclaredMethods(nameof(System.Linq.Enumerable.First))
            .Single(
                mi => mi.GetGenericArguments().Length == 1
                      && mi.GetParameters().Length == 1);

    internal static readonly MethodInfo FirstOrDefaultMethodInfo
        = typeof(System.Linq.Enumerable)
            .GetTypeInfo().GetDeclaredMethods(nameof(System.Linq.Enumerable.FirstOrDefault))
            .Single(
                mi => mi.GetGenericArguments().Length == 1
                      && mi.GetParameters().Length == 1);
    
    internal static readonly MethodInfo SelectMethodInfo
        = typeof(System.Linq.Enumerable)
            .GetTypeInfo().GetDeclaredMethods(nameof(System.Linq.Enumerable.Select))
            .Single(
                mi => mi.GetGenericArguments().Length == 2
                      && mi.GetParameters().Length == 2
                      && mi.GetParameters()[1].ParameterType.GetGenericArguments().Length == 2);

    internal static readonly MethodInfo SingleMethodInfo
        = typeof(System.Linq.Enumerable)
            .GetTypeInfo().GetDeclaredMethods(nameof(System.Linq.Enumerable.Single))
            .Single(
                mi => mi.GetGenericArguments().Length == 1
                      && mi.GetParameters().Length == 1);

    internal static readonly MethodInfo SingleOrDefaultMethodInfo
        = typeof(System.Linq.Enumerable)
            .GetTypeInfo().GetDeclaredMethods(nameof(System.Linq.Enumerable.SingleOrDefault))
            .Single(
                mi => mi.GetGenericArguments().Length == 1
                      && mi.GetParameters().Length == 1);
}

using System.Runtime.CompilerServices;

namespace BccCode.Linq.Client;

/// <summary>
/// Makes Native AOT precompile the generic LINQ methods that <see cref="ApiQueryProvider"/> instantiates at
/// runtime with <c>MakeGenericMethod</c>, for common value types.
/// </summary>
/// <remarks>
/// When a query projects to a value type (<c>Select(p =&gt; p.Age)</c>), the provider calls
/// <c>Enumerable.Select&lt;TEntity, int&gt;</c>, <c>Enumerable.First&lt;int&gt;</c> etc. Native AOT can only run
/// such an instantiation if its code was compiled ahead of time. For a reference type <c>TEntity</c> the code is
/// shared with <c>Select&lt;object, int&gt;</c>, so calling the <c>object</c> versions here is enough. Only
/// creating delegates to them was not: the runtime lookup behind <c>MakeGenericMethod</c> still failed.
/// Value types that are not listed (enums, user structs) are not covered.
/// The calls run once, on empty or one-element sequences, and only when dynamic code is not supported.
/// </remarks>
internal static class AotGenericRoots
{
    internal static void EnsureRooted()
    {
        // Constant true under Native AOT and false under JIT, so the JIT never runs this.
        if (RuntimeFeature.IsDynamicCodeSupported)
            return;

        Root<bool>(); Root<byte>(); Root<short>(); Root<int>(); Root<long>();
        Root<float>(); Root<double>(); Root<decimal>(); Root<char>(); Root<Guid>();
        Root<DateTime>(); Root<DateTimeOffset>(); Root<TimeSpan>(); Root<DateOnly>(); Root<TimeOnly>();

        Root<bool?>(); Root<byte?>(); Root<short?>(); Root<int?>(); Root<long?>();
        Root<float?>(); Root<double?>(); Root<decimal?>(); Root<char?>(); Root<Guid?>();
        Root<DateTime?>(); Root<DateTimeOffset?>(); Root<TimeSpan?>(); Root<DateOnly?>(); Root<TimeOnly?>();
    }

    private static void Root<T>()
    {
        // Select<TEntity, T> with a reference type TEntity, used for 'Select' to a value type.
        var selected = Enumerable.Select(Array.Empty<object>(), static _ => default(T)!);
        GC.KeepAlive(AsyncEnumerable.Select(EmptyAsync(), static _ => default(T)!));

        // First/Any/... after a 'Select' to a value type.
        GC.KeepAlive(Enumerable.Any(selected));
        GC.KeepAlive(Enumerable.FirstOrDefault(selected));
        GC.KeepAlive(Enumerable.SingleOrDefault(selected));
        var one = Enumerable.Repeat(default(T)!, 1);
        GC.KeepAlive(Enumerable.First(one));
        GC.KeepAlive(Enumerable.Single(one));
    }

    private static async IAsyncEnumerable<object> EmptyAsync()
    {
        await Task.CompletedTask;
        yield break;
    }
}

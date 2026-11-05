using System.Diagnostics.CodeAnalysis;
using System.Runtime.CompilerServices;
using BccCode.Linq.Client;
using BccCode.Platform;

// Smoke test for BccCode.Linq under Native AOT. The fake client returns the same rows for every request
// and records the request, like the mockup in BccCode.Linq.Tests. Filtering and sorting are done by the
// API in production, so they are checked on the recorded request. Select, First, Any etc. run locally.

var client = new FakeClient();
var failures = new List<string>();

// Queries that are known not to work under Native AOT. They are reported but do not fail the run when
// running without dynamic code support. When they work (or when running as JIT) they are checked normally.
void ReportFailure(string name, string message, bool knownAotLimitation)
{
    if (knownAotLimitation && !RuntimeFeature.IsDynamicCodeSupported)
    {
        Console.WriteLine($"KNOWN LIMITATION (Native AOT) {name}: {message}");
        return;
    }

    Console.WriteLine($"FAIL {name}: {message}");
    failures.Add(name);
}

void Run(string name, Func<object?> test, Func<object?, string?> check, bool knownAotLimitation = false)
{
    try
    {
        var result = test();
        var error = check(result);
        if (error == null) Console.WriteLine($"OK   {name}: {result}");
        else ReportFailure(name, $"{result} -> {error}", knownAotLimitation);
    }
    catch (Exception e)
    {
        ReportFailure(name, $"{e.GetType().Name}: {e.Message}", knownAotLimitation);
    }
}

async Task RunAsync(string name, Func<Task<object?>> test, Func<object?, string?> check, bool knownAotLimitation = false)
{
    try
    {
        var result = await test();
        var error = check(result);
        if (error == null) Console.WriteLine($"OK   {name}: {result}");
        else ReportFailure(name, $"{result} -> {error}", knownAotLimitation);
    }
    catch (Exception e)
    {
        ReportFailure(name, $"{e.GetType().Name}: {e.Message}", knownAotLimitation);
    }
}

string? Expect(object? actual, object? expected) => Equals(actual, expected) ? null : $"expected {expected}";

// GetQueryable<T> is annotated with RequiresDynamicCode / RequiresUnreferencedCode. This test exists to
// check what happens at runtime under Native AOT, so the warnings are suppressed here.
[UnconditionalSuppressMessage("AOT", "IL3050", Justification = "Smoke test of the annotated entry point.")]
[UnconditionalSuppressMessage("Trimming", "IL2026", Justification = "Smoke test of the annotated entry point.")]
IQueryable<Item> Items() => client.GetQueryable<Item>("items", p => client.LastParameters = (Parameters)p);

// Enums and user-defined structs are not precompiled by BccCode.Linq. An app roots them itself, as docs/aot.md
// describes; this checks that the documented line is enough.
_ = Enumerable.Select(Array.Empty<object>(), static _ => default(ItemKind)).ToList();

var minAge = 20;

Run("Where with captured variable",
    () =>
    {
        var list = Items().Where(i => i.Age > minAge).ToList();
        return $"{list.Count} rows, filter={client.LastParameters?.Filter}";
    },
    r => r is string s && s.Contains("20") && s.Contains("_gt") ? null : "filter does not contain the captured value");

Run("OrderBy",
    () =>
    {
        var list = Items().OrderBy(i => i.Name).ToList();
        return $"{list.Count} rows, sort={client.LastParameters?.Sort}";
    },
    r => r is string s && s.Contains("name") ? null : "sort does not contain the member name");

Run("Select anonymous type",
    () => string.Join(",", Items().Select(i => new { i.Name, i.Age }).ToList().Select(x => $"{x.Name}:{x.Age}")),
    r => Expect(r, "Anna:30,Bjorn:20,Cecilie:40"));

Run("Select int member",
    () => string.Join(",", Items().Select(i => i.Age).ToList()),
    r => Expect(r, "30,20,40"));

// Enumerable.Select<Item, int> and First<int> are instantiated at runtime with MakeGenericMethod. They work
// under Native AOT because BccCode.Linq precompiles them for common value types (AotGenericRoots).
Run("Select int member, then First",
    () => Items().Select(i => i.Age).First(),
    r => Expect(r, 30));

Run("Select nullable int",
    () => string.Join(",", Items().Select(i => (int?)i.Age).ToList()),
    r => Expect(r, "30,20,40"));

Run("Select Guid member",
    () => Items().Select(i => i.Key).ToList().Count,
    r => Expect(r, 3));

Run("Select enum member",
    () => string.Join(",", Items().Select(i => i.Kind).ToList()),
    r => Expect(r, "Adult,Adult,Adult"));

Run("First",
    () => Items().First().Name,
    r => Expect(r, "Anna"));

Run("FirstOrDefault",
    () => Items().FirstOrDefault()?.Name,
    r => Expect(r, "Anna"));

Run("Any",
    () => Items().Any(),
    r => Expect(r, true));

Run("Include",
    () =>
    {
        var list = Items().Include(i => i.Child).ToList();
        return $"{list.Count} rows, fields={client.LastParameters?.Fields}";
    },
    r => r is string s && s.Contains("child", StringComparison.OrdinalIgnoreCase) ? null : "fields do not contain the included member");

await RunAsync("ToListAsync",
    async () => (await Items().ToListAsync()).Count,
    r => Expect(r, 3));

await RunAsync("await foreach",
    async () =>
    {
        var names = new List<string>();
        await foreach (var item in Items().AsAsyncEnumerable())
            names.Add(item.Name);
        return string.Join(",", names);
    },
    r => Expect(r, "Anna,Bjorn,Cecilie"));

Console.WriteLine($"Dynamic code supported: {RuntimeFeature.IsDynamicCodeSupported}");
Console.WriteLine(failures.Count == 0 ? "No unexpected failures." : $"{failures.Count} queries failed: {string.Join(", ", failures)}");
return failures.Count == 0 ? 0 : 1;

public class Item
{
    public int Id { get; set; }
    public string Name { get; set; } = "";
    public int Age { get; set; }
    public Item? Child { get; set; }
    public Guid Key { get; set; } = Guid.NewGuid();
    public ItemKind Kind { get; set; } = ItemKind.Adult;
}

public enum ItemKind { Child, Adult }

public class Parameters : IQueryableParameters
{
    public string? Fields { get; set; }
    public string? Filter { get; set; }
    public string? Search { get; set; }
    public string? Sort { get; set; }
    public int? Limit { get; set; }
    public int? Offset { get; set; }
    public int? Page { get; set; }
    public string? Aggregate { get; set; }
    public string? GroupBy { get; set; }
    public string? Deep { get; set; }
    public string? Alias { get; set; }
    public string? Meta { get; set; }

    public IQueryableParameters Clone() => (Parameters)MemberwiseClone();
}

public class FakeClient : IHttpApiQueryableClient
{
    private static readonly List<Item> Data = new()
    {
        new Item { Id = 1, Name = "Anna", Age = 30 },
        new Item { Id = 2, Name = "Bjorn", Age = 20 },
        new Item { Id = 3, Name = "Cecilie", Age = 40 },
    };

    public Parameters? LastParameters { get; set; }

    public int? QueryBatchSize => 100;

    public IQueryableParameters CreateQueryableParameters(string path) => new Parameters();

    public TResult? Get<TResult>(string endpoint, IQueryableParameters query) where TResult : class
    {
        // Only one entity type is served, so no reflection is needed to create the result list.
        if (typeof(TResult) == typeof(ResultList<Item>))
            return (TResult)(object)new ResultList<Item>(Data, null);

        throw new NotSupportedException($"No data for {typeof(TResult).FullName}");
    }

    public Task<TResult?> GetAsync<TResult>(string endpoint, IQueryableParameters request, CancellationToken cancellationToken = default) where TResult : class
        => Task.FromResult(Get<TResult>(endpoint, request));
}

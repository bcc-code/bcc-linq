# Native AOT and trimming

Status: **`BccCode.Linq` (client)** builds with `IsAotCompatible=true` on net10.0 with 0 analyzer warnings
(for the parts that are dynamic by design, see below). **`BccCode.Linq.Server`** is not AOT- or trim-compatible
and does not set `IsAotCompatible`. It can be referenced from a `PublishAot=true` app, but the core features
will produce AOT/trim warnings in the app and may fail at runtime.

For the earlier analysis (in Norwegian) and the ReadyToRun recommendation, see [aotForLinq.md](aotForLinq.md).

## How this was measured

```bash
dotnet build src/BccCode.Linq/BccCode.Linq.csproj -c Release --no-incremental -p:IsAotCompatible=true
dotnet build src/BccCode.Linq.Server/BccCode.Linq.Server.csproj -c Release --no-incremental -p:IsAotCompatible=true
```

`IsAotCompatible=true` turns on the trim, AOT and single-file analyzers. Both packages target net10.0 only. The counts below are the IL warnings (IL2xxx, IL3xxx) reported by the .NET 10 SDK analyzers. They are higher than in the earlier measurement with the .NET 8 target, partly because the newer analyzers report more cases (for example IL2060 for `MakeGenericMethod`).

## BccCode.Linq (client): 0 warnings

Before this change the client had 39 analyzer warnings with the .NET 10 analyzers (24 with the earlier .NET 8
target). The project now sets `IsAotCompatible` and the build reports none.

Fixed:

- `Include`, `ThenInclude` and `Search` in `QueryableExtensions` took their `MethodInfo` through
  `MakeGenericMethod`. They now use the method of a statically typed delegate.
- `ApiQueryProvider` created `QueryablePagedEnumerable<T>` with `MakeGenericType` and `Activator`. `ApiQueryable<T>`
  now creates it itself (`IApiQueryable.CreateApiCaller`).
- `IApiCaller.AsQueryable()` is implemented in `QueryablePagedEnumerable<T>` with the generic
  `Queryable.AsQueryable<T>` instead of the non-generic overload.
- The `GetInterface(nameof(IEnumerable))` check is now `typeof(IEnumerable).IsAssignableFrom(...)`.
- The constant of a `Nullable<T>` is no longer read through reflection (a boxed `Nullable<T>` is already the
  underlying value).
- `TypeHelper`: the unused `IsAssignableToGenericType` is removed, and `IsNumberType` uses a static set of
  types (the earlier list plus `Half`, `nint`, `nuint`, `Int128`, `UInt128`) instead of looking for `INumber<>`
  through `GetInterfaces()`.
- `Evaluator` evaluated captured values through `Expression.Lambda(e)`, whose delegate type is only known at
  runtime, and `DynamicInvoke`. It now compiles a `Func<object?>`.

Dynamic by design:

- The provider translates the query's expression tree at runtime. The element and result types come from the
  user's expression (for example the anonymous or `int` result of `Select`), so `ApiQueryProvider` calls
  `MakeGenericMethod` on `Enumerable.Any/First/FirstOrDefault/Single/SingleOrDefault/Select` and on the async
  `Select` with those types. The non-generic `IQueryProvider.CreateQuery(Expression)` uses `MakeGenericType`
  the same way as `EnumerableQuery` in System.Linq. `Queryable.AsQueryable<T>` is annotated by the framework
  (IL2026, IL3050).
- The requirement is declared on the public entry point: `IQueryableApiClientExtension.GetQueryable<T>` has
  `[RequiresDynamicCode]` and `[RequiresUnreferencedCode]` (net7.0 and later). A Native AOT or trimmed app that
  calls it gets IL3050 and IL2026 at the call site.
- Inside the package the warnings are suppressed with `[UnconditionalSuppressMessage]` at method level
  (`ApiQueryProvider.VisitMethodCall`, `ApiQueryProvider.CreateQuery(Expression)`, `TypeHelper`,
  `QueryablePagedEnumerable<T>.AsQueryable`). Every query reaches `ApiQueryProvider` only through an
  `ApiQueryable<T>` created by `GetQueryable<T>`; both types are internal.

Not flagged by the analyzer, but still relevant under AOT:

- `Expression.Compile()` in `ApiQueryProvider.Execute*` and in `Evaluator` (partial evaluation of captured
  variables). Under Native AOT expression trees run in the interpreter: it works, but it is slower, and every
  query goes through it.
- Member names are read with `GetCustomAttribute<JsonPropertyNameAttribute>()` and
  `GetCustomAttributesData()` (for Newtonsoft's `JsonPropertyAttribute`, matched by type name) on the
  DTO's members. The members are reached through the query expression, so they are normally kept, but the
  attributes must survive trimming.
- The client does no JSON (de)serialisation itself; that is done by the consumer's `IHttpApiQueryableClient`
  implementation. Whether that path is AOT-safe depends on the consumer (for example System.Text.Json with a
  source-generated `JsonSerializerContext`).

### Value-type results (`Select(p => p.Age)`)

Native AOT can only run a generic method created with `MakeGenericMethod` if its code was compiled ahead of
time. For reference-type results (entities, anonymous types) the code is shared and always available. For
value-type results it is not: `Enumerable.Select<Person, int>` fails with
`NotSupportedException: ... is missing native code`.

`AotGenericRoots` makes the AOT compiler include `Select`, the async `Select`, `Any`, `First`,
`FirstOrDefault`, `Single` and `SingleOrDefault` for these result types and their `Nullable<>` versions:
`bool`, `byte`, `short`, `int`, `long`, `float`, `double`, `decimal`, `char`, `Guid`, `DateTime`,
`DateTimeOffset`, `TimeSpan`, `DateOnly`, `TimeOnly`. It runs once, from the static constructor of
`ApiQueryProvider`, and only when dynamic code is not supported (it is skipped under JIT).

- Size: on .NET 10 the smoke test executable grows from 3.75 MB to 4.19 MB (win-x64). Without it, `Select` to
  `int`, `int?` and `Guid` fails. (With the earlier .NET 8 target the same list added 5.5 MB.)
- Not covered: enums and user-defined structs. An app that selects such a type can add the instantiation
  itself, once, anywhere in code the AOT compiler sees, for example:
  `_ = Enumerable.Select(Array.Empty<object>(), static _ => default(MyEnum)).ToList();`

### Smoke test

`tests/BccCode.Linq.AotSmokeTest` is a console app with `PublishAot=true`. It uses an in-memory fake
`IHttpApiQueryableClient` and runs: `Where` with a captured variable, `OrderBy`, `Select` into an anonymous type,
`Select` of `int`, `int?`, `Guid` and enum members, `Select` followed by `First`, `First`, `FirstOrDefault`,
`Any`, `Include`, `ToListAsync` and `await foreach` over `AsAsyncEnumerable()`. The CI job `aot-smoke-test`
publishes it with Native AOT on linux-x64 and runs it; any failing query fails the job. The test roots its enum
with the line shown above, which also checks that this line is enough.

Result of `dotnet publish -r win-x64` (Native AOT, .NET 10) and under JIT: all queries work.

### Finding AOT problems in an app

AOT warnings are warnings, not errors: an app with problems still publishes and fails at runtime. Warnings from
NuGet packages are collapsed into one `IL2104: Assembly 'X' produced trim warnings` per assembly
(`-p:TrimmerSingleWarn=false` shows them). In an AOT app, consider:

```xml
<WarningsAsErrors>$(WarningsAsErrors);IL2026;IL2104;IL3050;IL3053</WarningsAsErrors>
```

and run the published binary in CI, as the smoke test does here.

## BccCode.Linq.Server: 57 warnings

| Warning | Count | Where | Cause |
|---|---|---|---|
| IL2026 + IL3050 | 22 (9 + 13) | `Filter` (11), `FilterDeserializationHelpers` (11) | reflection-based `System.Text.Json` `JsonSerializer.Serialize` / `Deserialize<T>` and `JsonStringEnumConverter` |
| IL3050 | 14 | `Filter` (`MakeGenericType(typeof(Filter<>), ...)`), `FilterToLambdaParser`, `OperandToExpressionResolver`, `CollectionsExtensions` | `MakeGenericType(typeof(Filter<>), ...)`, `MakeGenericMethod` for `OrderBy/ThenBy`, `Array.CreateInstance`, `Expression.Lambda` |
| IL2026 | 9 | `Internal/Queryable` (8), `FilterToLambdaParser:89` (1) | non-generic `Queryable.AsQueryable(IEnumerable)`; `Expression.Property(Expression, string)`: property looked up by name from the filter |
| IL2060 / IL2070 / IL2072 / IL2075 / IL2090 | 15 | `OperandToExpressionResolver` (11), `CollectionsExtensions` (4), `FilterInverter` (1) | `MakeGenericMethod`, `GetConstructor`, `GetField`, `GetProperty`, `Activator.CreateInstance` on unannotated types |

(The IL3050 count in the second row excludes the 13 System.Text.Json IL3050 warnings counted in the first row.)

The server package builds LINQ expressions from field names that arrive as strings in HTTP requests, so the
set of types, properties and generic instantiations is open-ended by design. This is the part that is
hardest to make AOT-safe.

## What it would take

Client (`BccCode.Linq`): the remaining limit is `Select` to enums and user-defined structs, which the app has to
root itself (see above). Removing it completely would need the provider to build the `Select`/`First`/`Any`/...
calls without `MakeGenericMethod` over the user's types.

Server (`BccCode.Linq.Server`), large effort; see the redesign in [aotForLinq.md](aotForLinq.md):

1. Replace the reflection-based System.Text.Json filter parsing with a source-generated context, or a small
   hand-written parser into a non-generic filter tree.
2. Replace `Filter<T>` built with `MakeGenericType` by that non-generic tree.
3. Replace string-based property lookup and `OrderBy` construction with per-DTO generated code (a source
   generator that emits property maps and sort/filter dispatch).

Until then, server apps should use ReadyToRun instead of Native AOT, as described in
[aotForLinq.md](aotForLinq.md).

# Native AOT and trimming

Status after the split into `BccCode.Linq` (client) and `BccCode.Linq.Server` (v2.0): **neither package is
AOT- or trim-compatible**, and neither sets `IsAotCompatible`. Both can be referenced from a `PublishAot=true`
app, but the core features will produce AOT/trim warnings in the app and may fail at runtime.

For the earlier analysis (in Norwegian) and the ReadyToRun recommendation, see [aotForLinq.md](aotForLinq.md).

## How this was measured

```bash
dotnet build src/BccCode.Linq/BccCode.Linq.csproj -c Release --no-incremental -p:IsAotCompatible=true
dotnet build src/BccCode.Linq.Server/BccCode.Linq.Server.csproj -c Release --no-incremental -p:IsAotCompatible=true
```

`IsAotCompatible=true` turns on the trim, AOT and single-file analyzers. Both packages target net10.0 only. The counts below are the IL warnings (IL2xxx, IL3xxx) reported by the .NET 10 SDK analyzers. They are higher than in the earlier measurement with the .NET 8 target, partly because the newer analyzers report more cases (for example IL2060 for `MakeGenericMethod`).

## BccCode.Linq (client): 39 warnings

| Warning | Count | Where | Cause |
|---|---|---|---|
| IL3050 | 19 | `ApiQueryProvider` (12), `QueryableExtensions` (4), `TypeHelper` (2), `Evaluator` (1) | `MakeGenericMethod` / `MakeGenericType` on `Enumerable.Any/First/Select/Single...`, `Include`, `IEnumerable<>`, `ApiQueryable<>` with types only known at runtime; `Expression.Lambda` in `Evaluator` |
| IL2060 | 13 | `ApiQueryProvider` (9), `QueryableExtensions` (4) | `MakeGenericMethod` whose requirements cannot be analyzed statically |
| IL2070 / IL2075 | 6 | `TypeHelper` (3), `ApiQueryProvider` (3) | `Type.GetInterfaces()`, `GetInterface(string)`, `GetProperty(string)` on unannotated types |
| IL2026 | 1 | `ApiQueryProvider:1682` | non-generic `Queryable.AsQueryable(IEnumerable)` |

Not flagged by the analyzer, but still a problem under AOT:

- `Expression.Compile()` in `ApiQueryProvider.Execute*` and in `Evaluator` (partial evaluation of captured
  variables). Under Native AOT expression trees run in the interpreter: it works, but it is slower, and every
  query goes through it.
- Member names are read with `GetCustomAttribute<JsonPropertyNameAttribute>()` and
  `GetCustomAttributesData()` (for Newtonsoft's `JsonPropertyAttribute`, matched by type name) on the
  DTO's members. The members are reached through the query expression, so they are normally kept, but the
  attributes must survive trimming.
- The client does no JSON (de)serialisation itself; that is done by the consumer's `IApiClient`
  implementation. Whether that path is AOT-safe depends on the consumer (for example System.Text.Json with a
  source-generated `JsonSerializerContext`).

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

Client (`BccCode.Linq`), moderate effort:

1. Replace `MakeGenericMethod` on `Enumerable`/`Queryable` methods with calls through generic helper methods
   (`static Expression Any<T>(...)`) dispatched once per element type, or annotate the public entry points
   with `[RequiresDynamicCode]` / `[RequiresUnreferencedCode]` so the warning moves to the caller's code.
2. Annotate `TypeHelper` and the `ApiQueryable<T>` type parameter with
   `[DynamicallyAccessedMembers(Interfaces | PublicProperties)]`.
3. Replace `Queryable.AsQueryable(IEnumerable)` with the generic overload.
4. Accept interpreted expression trees, or avoid `Compile()` for the common case where the evaluated value is a
   constant or a captured field.
5. Then set `<IsAotCompatible Condition="'$(TargetFramework)' == 'net10.0'">true</IsAotCompatible>` and keep the
   analyzer warnings at zero.

With 1–4 done the client could be marked AOT-compatible. The query provider itself is
`IQueryProvider`-based and does not need runtime code generation beyond what is listed above.

Server (`BccCode.Linq.Server`), large effort; see the redesign in [aotForLinq.md](aotForLinq.md):

1. Replace the reflection-based System.Text.Json filter parsing with a source-generated context, or a small
   hand-written parser into a non-generic filter tree.
2. Replace `Filter<T>` built with `MakeGenericType` by that non-generic tree.
3. Replace string-based property lookup and `OrderBy` construction with per-DTO generated code (a source
   generator that emits property maps and sort/filter dispatch).

Until then, server apps should use ReadyToRun instead of Native AOT, as described in
[aotForLinq.md](aotForLinq.md).

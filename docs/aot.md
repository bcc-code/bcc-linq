# Native AOT and trimming

Status after the split into `BccCode.Linq` (client) and `BccCode.Linq.Server` (v2.0): **neither package is
AOT- or trim-compatible**, and neither sets `IsAotCompatible`. Both can be referenced from a `PublishAot=true`
app, but the core features will produce AOT/trim warnings in the app and may fail at runtime.

For the earlier analysis (in Norwegian) and the ReadyToRun recommendation, see [aotForLinq.md](aotForLinq.md).

## How this was measured

```bash
dotnet build src/BccCode.Linq/BccCode.Linq.csproj -c Release -f net8.0 --no-incremental -p:IsAotCompatible=true
dotnet build src/BccCode.Linq.Server/BccCode.Linq.Server.csproj -c Release -f net8.0 --no-incremental -p:IsAotCompatible=true
```

`IsAotCompatible=true` turns on the trim, AOT and single-file analyzers.

## BccCode.Linq (client): 24 warnings

| Warning | Count | Where | Cause |
|---|---|---|---|
| IL3050 | 17 | `ApiQueryProvider` (11), `QueryableExtensions` (4), `TypeHelper` (2) | `MakeGenericMethod` / `MakeGenericType` on `Enumerable.Any/First/Select/Single...`, `Include`, `IEnumerable<>`, `ApiQueryable<>` with types only known at runtime |
| IL2070 / IL2075 | 5 | `TypeHelper` (3), `ApiQueryProvider` (2) | `Type.GetInterfaces()`, `GetInterface(string)`, `GetProperty(string)` on unannotated types |
| IL2026 + IL3050 | 2 | `ApiQueryProvider:1650` | non-generic `Queryable.AsQueryable(IEnumerable)` |

Not flagged by the analyzer, but still a problem under AOT:

- `Expression.Compile()` in `ApiQueryProvider.Execute*` and in `Evaluator` (partial evaluation of captured
  variables). Under Native AOT expression trees run in the interpreter: it works, but it is slower, and every
  query goes through it.
- Member names are read with `GetCustomAttribute<JsonPropertyNameAttribute / JsonPropertyAttribute>()` on the
  DTO's members. The members are reached through the query expression, so they are normally kept, but the
  attributes must survive trimming.
- The client does no JSON (de)serialisation itself; that is done by the consumer's `IApiClient`
  implementation. Whether that path is AOT-safe depends on the consumer (for example System.Text.Json with a
  source-generated `JsonSerializerContext`).

## BccCode.Linq.Server: 36 warnings

| Warning | Count | Where | Cause |
|---|---|---|---|
| IL2026 + IL3050 | 18 | `Filter`, `FilterDeserializationHelpers` | `Newtonsoft.Json` `JsonConvert.SerializeObject` / `DeserializeObject<T>` |
| IL3050 | 12 | `Filter`, `FilterToLambdaParser`, `OperandToExpressionResolver`, `CollectionsExtensions` | `MakeGenericType(typeof(Filter<>), ...)`, `MakeGenericMethod` for `OrderBy/ThenBy`, `Array.CreateInstance` |
| IL2026 | 1 | `FilterToLambdaParser:89` | `Expression.Property(Expression, string)`: property looked up by name from the filter |
| IL2060 / IL2070 / IL2090 | 5 | `OperandToExpressionResolver`, `CollectionsExtensions` | `GetConstructor`, `GetProperty` on unannotated types |

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
5. Then set `<IsAotCompatible Condition="'$(TargetFramework)' == 'net8.0'">true</IsAotCompatible>` and keep the
   analyzer warnings at zero.

With 1–4 done the client could be marked AOT-compatible. The query provider itself is
`IQueryProvider`-based and does not need runtime code generation beyond what is listed above.

Server (`BccCode.Linq.Server`), large effort; see the redesign in [aotForLinq.md](aotForLinq.md):

1. Replace Newtonsoft.Json filter parsing with System.Text.Json and a source-generated context, or a small
   hand-written parser into a non-generic filter tree.
2. Replace `Filter<T>` built with `MakeGenericType` by that non-generic tree.
3. Replace string-based property lookup and `OrderBy` construction with per-DTO generated code (a source
   generator that emits property maps and sort/filter dispatch).

Until then, server apps should use ReadyToRun instead of Native AOT, as described in
[aotForLinq.md](aotForLinq.md).

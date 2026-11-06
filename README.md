# BccCode.Linq
LINQ support for BCC API requests, shipped as two NuGet packages:

| Package | Use it in | Contains |
|---|---|---|
| [BccCode.Linq](https://www.nuget.org/packages/BccCode.Linq/) | API clients | LINQ query provider that turns queries into API requests (`BccCode.Linq.Client` namespace) |
| [BccCode.Linq.Server](https://www.nuget.org/packages/BccCode.Linq.Server/) | API servers | Directus-style filter parsing, sorting and paging on `IQueryable` (`BccCode.Linq.Server` namespace) |

Neither package depends on ASP.NET Core.

## Getting started
1. Add the package you need to a .NET project: `BccCode.Linq` for client code, `BccCode.Linq.Server` for server code (or both).
2. See the usage examples below.

## Usage on Client side (`BccCode.Linq`)

### Queryable usage

When you implement the IApiClient interface into your API client class, you are able to run linq queries
against it like this:

``` csharp
using BccCode.Linq;

IApiClient client = ...;

var persons = from person in client.Persons
              where person.Country == "NO"  // Note: Equals(person.Country, "NO") works, too
              select new
              {
                  person.Age,
                  person.Country
              };

foreach(var person in persons)
{
    // Here we interate through the query result getting
    // by default per API request 100 rows/persons.
}
```

## Usage on Server side (`BccCode.Linq.Server`)

### Filter usage
As an input param it takes a json string in directus filter format and applies the filter on a DbSet.

```csharp
    using BccCode.Linq.Server;

    var filter = new Filter(jsonFilter); // jsonFilter is a json string representation in a Directus Filter format

    var coll = await _dbContext.Collections
        .ApplyRuleFilter(filter)
        .ToListAsync();

```

## Migrating from 1.x

Version 2.0 moved the server code out of `BccCode.Linq` into `BccCode.Linq.Server`, and dropped the
`Microsoft.AspNetCore.Mvc` 2.x package references that the library never used.

- Projects that only use the client (`BccCode.Linq.Client`): update `BccCode.Linq` to 2.x. No code changes.
- Projects that use `Filter`, `Filter<T>`, `ApplyRuleFilter`, `ApplyApiRequest` or other types in
  `BccCode.Linq.Server`: also add a reference to `BccCode.Linq.Server` 2.x. Namespaces and type names are
  unchanged, so no code changes. Libraries compiled against 1.x that use these types must be recompiled,
  because the types now live in `BccCode.Linq.Server.dll`.
- Projects that got `Microsoft.AspNetCore.Mvc` types (for example `[FromQuery]`) only through
  `BccCode.Linq`: reference them directly, preferably with
  `<FrameworkReference Include="Microsoft.AspNetCore.App" />`.
- Both packages now target net10.0 only (1.x also targeted netstandard2.1, net6.0 and net8.0). Projects on
  older .NET versions must upgrade to .NET 10.
- The packages no longer depend on `Newtonsoft.Json` directly. The `BccCode.Platform.Contracts` dependency
  still brings it in. The client still reads custom member names from both
  `System.Text.Json.Serialization.JsonPropertyNameAttribute` and Newtonsoft's `JsonPropertyAttribute`.
- The server code now parses filters with `System.Text.Json`. The values in the dictionaries of `Filter` and
  of `FilterDeserializationHelpers.DeserializeJsonRule` are `System.Text.Json.JsonElement` instead of
  Newtonsoft tokens (`JToken`, `JArray`, `JObject`). Code that reads `Filter.Properties` or the result of
  `DeserializeJsonRule` and casts values to Newtonsoft types must be changed.

See [docs/aot.md](docs/aot.md) for Native AOT and trimming support.

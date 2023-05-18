# Directus Filter - C# implementation
This is an implementation of a parser and converter for  [Directus Filter](https://docs.directus.io/reference/query.html#filter). It parses a JSON representation of the Directus Filter format into a list of Expressions used in IQueryable, and it can also perform the reverse conversion.

## Getting started
1. Add [nuget package](https://www.nuget.org/packages/BccCode.RuleFilterParser/) to a .NET project
2. See the usage examples below.

## Filter usage
As an input param it takes a json string in directus filter format and applies the filter on a DbSet.

```csharp
    var filter = new Filter(jsonFilter); // jsonFilter is a json string representation in a Directus Filter format

    var coll = await _dbContext.Collections
        .ApplyRuleFilter(filter)
        .ToListAsync();

```

## Queryable usage

When you implement the IApiClient interface into your API client class, you are able to run linq queries
against it like this:

``` csharp
using RuleFilterParser;

IApiClient client = ...;

var queryable = client.GetAsQueryable("persons");  // URL path of the API

var persons = from person in queryable
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

## DirectusFilterBuilder usage

DirectusFilterBuilder is a C# implementation for building a json string in a Directus Filter format.

```csharp
  var json = DirectusFilterBuilder<Project>.Create()
            .Where(x => x.Status == true)
            .Serialize();
```
```csharp
  var json = DirectusFilterBuilder<Project>.Create()
            .Where(x => x.Start >= new DateTime(2023, 5, 1) && x.Start <= new DateTime(2023, 5, 31));
            .Serialize();
```
Results in `{"Status":{"_eq":"True"}}` and `{"Start":{"_gte":"2023-05-01T00:00:00.0000000","_lte":"2023-05-31T00:00:00.0000000"}}` respectively.

`Project` is POCO object and could be any object like an entity.

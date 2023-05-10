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

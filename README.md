# BccCode.Linq
This is an implementation for Linq for BCC API requests on client and server side.

## Getting started
1. Add nuget package [BccCode.Linq](https://www.nuget.org/packages/BccCode.Linq/) to a .NET project
2. See the usage examples below.

## Usage on Client side

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

## Usage on Server side

### Filter usage
As an input param it takes a json string in directus filter format and applies the filter on a DbSet.

```csharp
    var filter = new Filter(jsonFilter); // jsonFilter is a json string representation in a Directus Filter format

    var coll = await _dbContext.Collections
        .ApplyRuleFilter(filter)
        .ToListAsync();

```

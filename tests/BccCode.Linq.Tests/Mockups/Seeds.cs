using System.Collections.Immutable;
using BccCode.Linq.Tests.Helpers;

namespace BccCode.Linq.Tests;

internal static class Seeds
{
    private static IReadOnlyCollection<Person>? _persons;
    private static IReadOnlyCollection<ManufacturerInfo>? _manufacturers;

    public static IReadOnlyCollection<Person> Persons
        => _persons ??= ImmutableList.Create(new Person[]
        {
            new("Archibald Mcbride", 18, "US", new DateTime(2020, 1, 4)),
            new("Tim Obrien", 25, "NO", new DateTime(1942, 5, 23)),
            new("Chelsey Logan", 22, "IE", new DateTime(1958, 5, 2))
            {
                Car = new Car("Opel", "Astra", 2003)
            },
            new("Reid Cantrell", 54, "DE", new DateTime(1942, 7, 23))
            {
                Car = new Car("Opel", "Astra", 2019)
            },
            new("Amelie Beasley", 75, "PL", new DateTime(1982, 12, 24))
            {
                Car = new Car("Volkswagen", "Golf", 2020)
            },
        });

    public static IReadOnlyCollection<ManufacturerInfo> Manufacturers
        => _manufacturers ??= ImmutableList.Create(new[]
        {
            new ManufacturerInfo { Uid = new Guid("4477e983-be5c-43d5-b3d0-5f26971bf2f3"), Name = "Opel", EstablishedYear = 1862 },
            new ManufacturerInfo { Uid = new Guid("16b41e40-ee3c-4837-b9a9-57b76c7d1d9d"), Name = "Volkswagen", EstablishedYear = 1937 }
        });
}

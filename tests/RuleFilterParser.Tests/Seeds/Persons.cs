using RuleToLinqParser.Tests.Helpers;

namespace RuleToLinqParser.Tests.Seeds;

internal static class Persons
{
    public static List<Person> TestData()
    {
        return new List<Person>
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
            new("Amelie Beasley", 75, "PL", new DateTime(1982, 12, 24)),
        };
    }
}
namespace RuleToLinqParser.Tests.Helpers;

public class Person
{
    public string Name { get; set; }
    public int Age { get; set; }
    public string Country { get; set; }
    public DateTime AnyDate { get; set; }

    public Person(string name, int age, string country, DateTime anyDate)
    {
        Name = name;
        Age = age;
        Country = country;
        AnyDate = anyDate;
    }
}
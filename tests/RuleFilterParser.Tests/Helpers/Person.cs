namespace RuleToLinqParser.Tests.Helpers;

public class Person
{
    public string Name { get; set; }
    public int Age { get; set; }
    public string Country { get; set; }

    public Car Car { get; set; } = new Car("Opel", "Astra", 2003);
    public DateTime AnyDate { get; set; }

    public Person(string name, int age, string country, DateTime anyDate)
    {
        Name = name;
        Age = age;
        Country = country;
        AnyDate = anyDate;
    }
}

public class Car
{
    public string Manufacturer { get; set; }
    public string Model { get; set; }
    public int YearOfProduction { get; set; }

    public Car(string manufacturer, string model, int yearOfProduction)
    {
        Manufacturer = manufacturer;
        Model = model;
        YearOfProduction = yearOfProduction;
    }
}
namespace BccCode.Linq.Tests.Helpers;

public class Person
{
    public string Name { get; set; }
    public int Age { get; set; }
    public string Country { get; set; }

    public List<Car> CarHistory { get; set; } = new List<Car>
    {
        new("Opel", "Astra", 2003),
        new("Opel", "Rekord", 1985),
    };

    public Car Car { get; set; } = new("Opel", "Astra", 2003);
    public DateTime AnyDate { get; set; }

    public PersonType Type { get; set; }

    public Person(string name, int age, string country, DateTime anyDate)
    {
        Name = name;
        Age = age;
        Country = country;
        AnyDate = anyDate;
    }
}

public enum PersonType
{
    Unknown = 0,
    Customer = 1,
    Staff = 2,
    Partner = 3
}

public class Car
{
    public string Manufacturer { get; set; }
    public string Model { get; set; }
    public int YearOfProduction { get; set; }

    public Car(string manufacturer, string model, int yearOfProduction)
    {
        Manufacturer = manufacturer;
        ManufacturerInfo = new ManufacturerInfo { Name = manufacturer };
        Model = model;
        YearOfProduction = yearOfProduction;
    }

    public ManufacturerInfo ManufacturerInfo { get; set; }
}

public class ManufacturerInfo
{
    public Guid Uid { get; set; }
    public string Name { get; set; }
    public int EstablishedYear { get; set; }
}

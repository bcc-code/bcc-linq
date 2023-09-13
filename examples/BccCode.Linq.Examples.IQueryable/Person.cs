namespace BccCode.Linq.Examples.IQueryable;

public class Person
{
    public int Id { get; set; }
    public string Name { get; set; }
    public int Age { get; set; }
    public DateTime SomeDate { get; set; }

    public List<Relation> Relations { get; set; }

    public Person(string name, int age, DateTime someDate)
    {
        Name = name;
        Age = age;
        SomeDate = someDate;
    }
}

public class Relation
{
    public Person Target {  get; set;}
}

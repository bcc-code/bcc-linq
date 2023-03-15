namespace RuleFilterParser.Examples.IQueryable;

public class Person
{
    public int Id { get; set; }
    public string Name { get; set; }
    public int Age { get; set; }

    public Person(string name, int age)
    {
        Name = name;
        Age = age;
    }
}
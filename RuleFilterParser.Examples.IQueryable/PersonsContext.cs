using Microsoft.EntityFrameworkCore;

namespace RuleFilterParser.Examples.IQueryable;

public class PersonsContext : DbContext
{
    public DbSet<Person> Persons { get; set; }
    
    public PersonsContext(DbContextOptions options) : base(options) { }
}
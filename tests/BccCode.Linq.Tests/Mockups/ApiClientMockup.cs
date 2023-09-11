using BccCode.Linq.Tests.Helpers;
using BccCode.Linq.Client;

namespace BccCode.Linq.Tests;

public class ApiClientMockup : ApiClientMockupBase
{
    public ApiClientMockup()
    {
        // registering seeds data ...
        RegisterData(typeof(Person), Seeds.Persons);
        RegisterData(typeof(TestClass), Array.Empty<TestClass>());
        RegisterData(typeof(ManufacturerInfo), Seeds.Manufacturers);
    }

    // strongly typed entities
    public IQueryable<Person> Persons => this.GetQueryable<Person>("persons", a => this.ClientQuery = a);
    public IQueryable<TestClass> Empty => this.GetQueryable<TestClass>("empty", a => this.ClientQuery = a);
    public IQueryable<ManufacturerInfo> Manufacturers => this.GetQueryable<ManufacturerInfo>("manufacturers", a => this.ClientQuery = a);
}

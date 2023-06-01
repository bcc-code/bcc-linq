using RuleFilterParser;
using RuleToLinqParser.Tests.Helpers;

namespace RuleToLinqParser.Tests;

public class ApiClientMockup : ApiClientMockupBase
{
    public ApiClientMockup()
    {
        // registering seeds data ...
        RegisterData(typeof(Person), Seeds.Persons);
    }

    // strongly typed entities
    public IQueryable<Person> Persons => this.GetQueryable<Person>("persons");
}

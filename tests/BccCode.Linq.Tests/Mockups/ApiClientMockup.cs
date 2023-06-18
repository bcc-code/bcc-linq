﻿using BccCode.Linq.ApiClient;
using BccCode.Linq.Tests.Helpers;

namespace BccCode.Linq.Tests;

public class ApiClientMockup : ApiClientMockupBase
{
    public ApiClientMockup()
    {
        // registering seeds data ...
        RegisterData(typeof(Person), Seeds.Persons);
        RegisterData(typeof(ManufacturerInfo), Seeds.Manufacturers);
    }

    // strongly typed entities
    public IQueryable<Person> Persons => this.GetQueryable<Person>("persons");
    public IQueryable<ManufacturerInfo> Manufacturers => this.GetQueryable<ManufacturerInfo>("manufacturers");
}

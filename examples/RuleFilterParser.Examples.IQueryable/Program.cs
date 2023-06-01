using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using RuleFilterParser;
using RuleFilterParser.Examples.IQueryable;
using RuleFilterParser.Extensions;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var inMemoryDb = builder.Configuration.GetValue<bool>("Database:UseInMemoryDatabase");
if (inMemoryDb)
{
    builder.Services.AddDbContext<PersonsContext>(options =>
        options.UseInMemoryDatabase("persons"));
}
else
{
    var connectionString = builder.Configuration.GetValue<string>("Database:Postgres");
    builder.Services.AddDbContext<PersonsContext>(options =>
        options.UseNpgsql(connectionString));
}


var app = builder.Build();
AppContext.SetSwitch("Npgsql.EnableLegacyTimestampBehavior", true);

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

// Filter example
// "{ "age": { "_between": [2, 4] } }";
// "{ "someDate": { "_between": ["2023-03-14T00:00:00.0000000", "2023-03-16T00:00:00.0000000"] } }";
app.MapGet("/persons", async ([FromQuery] string jsonFilter, PersonsContext db) =>
    await db.Persons
        .ApplyRuleFilter(new Filter<Person>(jsonFilter))
        .ToListAsync());

app.MapPost("/persons/seed", async (PersonsContext db) =>
{
    await db.Persons.AddRangeAsync(new List<Person>
    {
        new("Kobe Bryant", 1, DateTime.UtcNow.AddDays(-1)),
        new("Michael Jordan", 2, DateTime.UtcNow.AddDays(-2)),
        new("Scottie Pippen", 3, DateTime.UtcNow.AddDays(-3)),
        new("LeBron James", 4, DateTime.UtcNow.AddDays(-4)),
        new("Kevin Garnett", 5, DateTime.UtcNow.AddDays(-5)),
        new("Ben Wallace", 6, DateTime.UtcNow.AddDays(-6))
    });

    await db.SaveChangesAsync();
});

app.Run();
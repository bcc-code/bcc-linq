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
app.MapGet("/persons", async ([FromQuery] string jsonFilter, PersonsContext db) =>
    await db.Persons
        .ApplyRuleFilter(new Filter(jsonFilter))
        .ToListAsync());

app.MapPost("/persons/seed", async (PersonsContext db) =>
{
    await db.Persons.AddAsync(new("Kobe Bryant", 1));
    await db.Persons.AddAsync(new("Michael Jordan", 2));
    await db.Persons.AddAsync(new("Scottie Pippen", 3));
    await db.Persons.AddAsync(new("LeBron James", 4));
    await db.Persons.AddAsync(new("Kevin Garnett", 5));
    await db.Persons.AddAsync(new("Ben Wallace", 6));

    await db.SaveChangesAsync();
});

app.Run();
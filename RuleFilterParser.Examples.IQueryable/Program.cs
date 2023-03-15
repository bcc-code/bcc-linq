using Microsoft.EntityFrameworkCore;
using RuleFilterParser.Examples.IQueryable;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.AddDbContext<PersonsContext>(options => options.UseInMemoryDatabase("persons"));


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

app.MapGet("/persons", async (PersonsContext db) => await db.Persons.ToListAsync());

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
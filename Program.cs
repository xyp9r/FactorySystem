using Microsoft.EntityFrameworkCore;
using FactorySystem.Data;
using FactorySystem.Models;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddOpenApi();

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.AddDbContext<AppDbContext>( opt => opt.UseSqlite("Data Source=factory.db"));

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.UseHttpsRedirection();

app.MapGet("/api/factory/ping", () => "Завод успешно запущен и готов к работе!");

app.MapPost("/api/factory/users", (User newUser, AppDbContext db) =>
{
    db.Users.Add(newUser);
    db.SaveChanges();
    return Results.Ok(newUser);
});

app.MapGet("/api/factory/users", (AppDbContext db) =>
{
    var allUsers = db.Users.ToList();
    return Results.Ok(allUsers);
});

app.Run();

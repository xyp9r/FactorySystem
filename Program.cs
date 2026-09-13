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

// Пинг проверки
app.MapGet("/api/factory/ping", () => "Завод успешно запущен и готов к работе!");

// Отправляем юзеров в бд
app.MapPost("/api/factory/users", (User newUser, AppDbContext db) =>
{
    db.Users.Add(newUser);
    db.SaveChanges();
    return Results.Ok(newUser);
});

// получаем юзеров из бд
app.MapGet("/api/factory/users", (AppDbContext db) =>
{
    var allUsers = db.Users.ToList();
    return Results.Ok(allUsers);
});

// отправляем детали
app.MapPost("/api/factory/details", (Detail newDetail, AppDbContext db) =>
{
    db.Details.Add(newDetail);
    db.SaveChanges();
    return Results.Ok(newDetail);
});

// получаем детали
app.MapGet("/api/factory/details", (AppDbContext db) =>
{
    var allDetails = db.Details.ToList();
    return Results.Ok(allDetails);
});

app.Run();

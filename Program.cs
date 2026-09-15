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

// Удаление юзеров
app.MapDelete("/api/factory/users/{id}", (int id, AppDbContext db) =>
{
    var deleteUser = db.Users.Find(id);
    if (deleteUser != null)
    {
        db.Users.Remove(deleteUser);
        db.SaveChanges();
        return Results.Ok(deleteUser);
    }
    else
    {
        return Results.NotFound();
    }
});

// отправляем детали
app.MapPost("/api/factory/details", (CreateDetailDto dto, AppDbContext db) =>
{
    var detail = new Detail
    {
        Name = dto.Name,
        Count = dto.Count,
        CreatorId = dto.CreatorId,
        Status = dto.Status
    };

    db.Details.Add(detail);
    db.SaveChanges();
    return Results.Ok(detail);  
});

// получаем детали
app.MapGet("/api/factory/details", (AppDbContext db) =>
{
    var allDetails = db.Details.Include(d => d.Creator).ToList();
    return Results.Ok(allDetails);
});

// обновляем детали
app.MapPut("/api/factory/details/{id}", (int id, CreateDetailDto dto, AppDbContext db) =>
{
    var detail = db.Details.Find(id);

    if (detail != null)
    {
        detail.Name = dto.Name;
        detail.Count = dto.Count;
        detail.Status = dto.Status;
        detail.CreatorId = dto.CreatorId;

        db.SaveChanges();
        
        return Results.Ok(detail);
    }
    else
    {
        return Results.NotFound();
    }
});

app.Run();

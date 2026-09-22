using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using FactorySystem;
using Microsoft.EntityFrameworkCore;
using FactorySystem.Data;
using FactorySystem.Models;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi;
using Scalar.AspNetCore;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddDbContext<AppDbContext>( opt => opt.UseSqlite("Data Source=factory.db"));

builder.Services.AddAuthorization(); // Включаем систему прав

builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        var key = builder.Configuration["JwtSecret"];
        var secretBytes = System.Text.Encoding.UTF8.GetBytes(key);

        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidIssuer = "MyFactory", // Должно совпадать с тем что мы выдаем
            ValidateAudience = true,
            ValidAudience = "MyFactory", // Должно совпадать
            ValidateLifetime = true, // Проверяем срок годности
            IssuerSigningKey = new SymmetricSecurityKey(secretBytes), // Тот самый ключ
            ValidateIssuerSigningKey = true // Строго проверяем подпись
        };
    });

builder.Services.AddOpenApi(options =>
{
    options.AddDocumentTransformer((document, context, cancellationToken) =>
    {
        // 1. Создаем схему
        var scheme = new OpenApiSecurityScheme
        {
            Type = SecuritySchemeType.Http,
            Scheme = "bearer",
            BearerFormat = "JWT"
        };
        
        // 2. Безопасно инициализируем компоненты И сам словарь внутри них
        document.Components ??= new OpenApiComponents();
        document.Components.SecuritySchemes ??= new Dictionary<string, IOpenApiSecurityScheme>();
        
        // 3. Теперь безопасно добавляем схему в словарь
        document.Components.SecuritySchemes["bearer"] = scheme;
        
        // 4. Инициализируем и добавляем глобальное требование
        document.Security ??= new List<OpenApiSecurityRequirement>();
        document.Security.Add(new OpenApiSecurityRequirement
        {
            [new OpenApiSecuritySchemeReference("bearer", document)] = []
        });
        
        return Task.CompletedTask;
    });
});

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
    app.MapScalarApiReference();
}

app.UseHttpsRedirection();

app.UseAuthentication(); // Сначала проверяем паспорт (токен)
app.UseAuthorization(); // Затем проверяем права доступа

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
app.MapPost("/api/factory/details", (CreateDetailDto dto, AppDbContext db, ClaimsPrincipal userPrincipal) =>
{
    
    // читаем имя из токена
    var currentUserName = userPrincipal.FindFirstValue(ClaimTypes.Name);
    
    // Делаем запрос к бозе - ищем пользователя у которого Name = currentUserName
    var userFromDb = db.Users.FirstOrDefault(u => u.Name == currentUserName);
    
    // Защита от дурака: а вдруг юзера уже удалили из базы, а токен у него ещё жив
    if (userFromDb == null)
    {
        return Results.Unauthorized();
    }
    
    // Создаем теперь только деталь после всех проверок
    var detail = new Detail
    {
        Name = dto.Name,
        Count = dto.Count,
        Status = dto.Status,
        CreatorId = userFromDb.Id
    };

    db.Details.Add(detail);
    db.SaveChanges();
    return Results.Ok(detail);  
})
.RequireAuthorization();

// получаем детали
app.MapGet("/api/factory/details/", (AppDbContext db) =>
{
    var allDetails = db.Details.Include(d => d.Creator).ToList();
    return Results.Ok(allDetails);
})
.RequireAuthorization();

// обновляем детали
app.MapPut("/api/factory/details/{id}", (int id, CreateDetailDto dto, AppDbContext db,  ClaimsPrincipal userPrincipal) =>
{
    
    // читаем имя из токена
    var currentUserName = userPrincipal.FindFirstValue(ClaimTypes.Name);
    
    // Делаем запрос к бозе - ищем пользователя у которого Name = currentUserName
    var userFromDb = db.Users.FirstOrDefault(u => u.Name == currentUserName);
    
    // Защита от дурака: а вдруг юзера уже удалили из базы, а токен у него ещё жив
    if (userFromDb == null)
    {
        return Results.Unauthorized();
    }
    
    // находим айди детали
    var detail = db.Details.Find(id);

    // Проверяем существует ли деталь
    if (detail == null)
    {
        return Results.NotFound();
    }
    
    // проверяем
    if (detail.CreatorId != userFromDb.Id)
    {
        return Results.Forbid();
    }
    
    detail.Name = dto.Name;
    detail.Count = dto.Count;
    detail.Status = dto.Status;

    db.SaveChanges();
    return Results.Ok(detail);
})
.RequireAuthorization();

// Проверка на безопасность вход 
app.MapPost("/api/factory/login", (AppDbContext db, LoginDto ldto, IConfiguration config) =>
{
    var user = db.Users.FirstOrDefault(u => u.Name == ldto.Name);

    if (user == null)
    {
        return Results.Unauthorized();
    }

    if (user.PasswordHash != ldto.Password)
    {
        return Results.Unauthorized();
    }
        
    var key = config["JwtSecret"]!;
    var secretBytes = System.Text.Encoding.UTF8.GetBytes(key);
    
    var securityKey = new SymmetricSecurityKey(secretBytes);
    var credentials = new SigningCredentials(securityKey, SecurityAlgorithms.HmacSha256);

    List<Claim> claims = new List<Claim>();
    claims.Add(new Claim(ClaimTypes.Name, user.Name));
    claims.Add(new Claim(ClaimTypes.Role, user.Role));

    var token = new JwtSecurityToken(issuer: "MyFactory", audience: "MyFactory", claims, expires: DateTime.UtcNow.AddHours(1), signingCredentials: credentials);
    
    var jwtString = new JwtSecurityTokenHandler().WriteToken(token);
    
    return Results.Ok(new { token = jwtString });
});

app.Run();

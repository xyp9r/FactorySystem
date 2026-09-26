using Microsoft.EntityFrameworkCore;
using FactorySystem.Data;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi;
using Scalar.AspNetCore;
using FactorySystem.Endpoints;
using FactorySystem.Models;
using FactorySystem.Validators;
using FluentValidation;

var builder = WebApplication.CreateBuilder(args);

// Регистрация BCrypt хэшера
builder.Services.AddSingleton<IPasswordHasher, BCryptPasswordHasher>();

// Регистрация валидатора 
builder.Services.AddValidatorsFromAssemblyContaining<Program>(); 

builder.Services.AddDbContext<AppDbContext>( opt => opt.UseSqlite("Data Source=factory.db"));

builder.Services.AddAuthorization(options =>
{
    options.AddPolicy("Boss", policy =>
    {
        policy.RequireRole("Boss");
    });
}); // Включаем систему прав

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

app.MapUserEndpoints();
app.MapDetailsEndpoints();

app.Run();

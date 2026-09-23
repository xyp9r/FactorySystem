namespace FactorySystem.Endpoints;

using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using FactorySystem.Data;
using FactorySystem.Models;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using Microsoft.IdentityModel.Tokens;

public static class UserEndpoints
{

    public static WebApplication MapUserEndpoints(this WebApplication app)
    {
        
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
        })
        .RequireAuthorization();
        
        // Отправляем юзеров в бд
        app.MapPost("/api/factory/users", (User newUser, AppDbContext db) =>
        {
            db.Users.Add(newUser);
            db.SaveChanges();
            return Results.Ok(newUser);
        })
        .RequireAuthorization();

// получаем юзеров из бд
        app.MapGet("/api/factory/users", (AppDbContext db) =>
        {
            var allUsers = db.Users.ToList();
            return Results.Ok(allUsers);
        })
        .RequireAuthorization();

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
        })
        .RequireAuthorization();

        return app;
    }
}
using FactorySystem.Services;

namespace FactorySystem.Endpoints;

using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using FactorySystem.Data;
using FactorySystem.Models;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using Microsoft.IdentityModel.Tokens;
using BCrypt.Net;

public static class UserEndpoints
{

    public static WebApplication MapUserEndpoints(this WebApplication app)
    {
        
        // Проверка на безопасность вход 
        app.MapPost("/api/factory/login", (AuthService authService, LoginDto ldto) =>
        {
            var login = authService.Login(ldto);

            if (login == null)
            {
                return Results.Json(
                    data: new { error = "Неавторизован", message = "Неверный логин или пароль" }, 
                    statusCode: 401);
            }
            
            return Results.Ok(login);
        });
        
        // Отправляем юзеров в бд
        app.MapPost("/api/factory/users", (User newUser, UserService userService) =>
        {
            var safeUser = userService.CreateUser(newUser);
            
            return Results.Ok(safeUser);
        })
        .RequireAuthorization("Boss");

        // получаем юзеров из бд
        app.MapGet("/api/factory/users", (AppDbContext db) =>
            {
                
                var allUsers = db.Users
                    .Select(userFromDb => new UserResponseDto
                    {
                        Id = userFromDb.Id,
                        Name = userFromDb.Name,
                        Role = userFromDb.Role,
                    })
                    .ToList();
                    return Results.Ok(allUsers);
        })
        .RequireAuthorization();

        // Удаление юзеров
        app.MapDelete("/api/factory/users/{id}", (int id, AppDbContext db) =>
        {
            var deleteUser = db.Users.Find(id);
            if (deleteUser != null)
            {
                var safeUser = new UserResponseDto
                {
                    Id = deleteUser.Id,
                    Name = deleteUser.Name,
                    Role = deleteUser.Role
                };
                
                db.Users.Remove(deleteUser);
                db.SaveChanges();
                return Results.Ok(safeUser);
            }
            else
            {
                return Results.NotFound();
            }
        })
        .RequireAuthorization("Boss");

        return app;
    }
}
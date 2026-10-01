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
        app.MapGet("/api/factory/users", (UserService userService) =>
            {
                var getUsersAll = userService.GetUsersAll();
                
                return Results.Ok(getUsersAll);
            })
        .RequireAuthorization();

        // Получаем КОНКРЕТНОГО юзера
        app.MapGet("/api/factory/users/{id}", (UserService userService, int id) =>
        {
            var getUser = userService.GetUser(id);

            if (getUser != null)
            {
                return Results.Ok(getUser);
            }
            else
            {
                return Results.NotFound();
            }
        });

        app.MapPut("api/factory/users/{id}", (UserService userService, int id, UpdateUserDto dto) =>
            {

                var putUser = userService.UpdateUser(id, dto);
            
                if (putUser)
                {
                    return Results.Ok(putUser);
                }
                else 
                {
                    return Results.NotFound();
                }
            })
        .RequireAuthorization("Boss");

        // Удаление юзеров
        app.MapDelete("/api/factory/users/{id}", (UserService userService, int id) =>
        {
           var isDeleted = userService.DeleteUser(id);
           if (isDeleted)
           {
               return Results.Ok();
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
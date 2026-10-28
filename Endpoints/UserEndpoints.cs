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

            return login switch
            {
                { IsSuccess: true } => Results.Ok(login.Value),
                { Error.Type: ErrorType.Unauthorized } => Results.Unauthorized(),
                _ => Results.StatusCode(500)
            };
        });
        
        // Отправляем юзеров в бд
        app.MapPost("/api/factory/users", (User newUser, UserService userService) =>
        {
            var safeUser = userService.CreateUser(newUser);

            return safeUser switch
            {
                { IsSuccess: true } => Results.Ok(safeUser.Value),
                { Error.Type: ErrorType.Conflict } => Results.Conflict(safeUser.Error.Message),
                _ => Results.StatusCode(500)
            };
        })
        .RequireAuthorization("Boss");

        // получаем юзеров из бд
        app.MapGet("/api/factory/users", (UserService userService) =>
            {
                var getUsersAll = userService.GetUsersAll();
                
                return getUsersAll switch
                {
                    { IsSuccess: true } => Results.Ok(getUsersAll.Value),
                    _ => Results.StatusCode(500)
                };
            })
        .RequireAuthorization();

        // Получаем КОНКРЕТНОГО юзера
        app.MapGet("/api/factory/users/{id}", (UserService userService, int id) =>
        {
            var getUser = userService.GetUser(id);

            return getUser switch
            {
                { IsSuccess: true } => Results.Ok(getUser.Value),
                { Error.Type: ErrorType.NotFound } => Results.NotFound(getUser.Error.Message),
                _ => Results.StatusCode(500)
            };
        });

        // Обновляем конкретного юзера
        app.MapPut("api/factory/users/{id}", (UserService userService, int id, UpdateUserDto dto) =>
            {

                var putUser = userService.UpdateUser(id, dto);

                return putUser switch
                {
                    { IsSuccess: true } => Results.Ok(putUser.Value),
                    { Error.Type: ErrorType.NotFound } => Results.NotFound(putUser.Error.Message),
                    _ => Results.StatusCode(500)
                };
            })
        .RequireAuthorization("Boss");

        // Удаление юзеров
        app.MapDelete("/api/factory/users/{id}", (UserService userService, int id) =>
        {
           var userDeleted = userService.DeleteUser(id);

           return userDeleted switch
           {
               {IsSuccess: true} => Results.Ok(),
               { Error.Type: ErrorType.NotFound } => Results.NotFound(userDeleted.Error.Message),
               _ => Results.StatusCode(500)
           };

        })
        .RequireAuthorization("Boss");

        return app;
    }
}
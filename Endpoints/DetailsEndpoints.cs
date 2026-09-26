namespace FactorySystem.Endpoints;

using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using FactorySystem.Data;
using FactorySystem.Models;
using System.Security.Claims;
using Microsoft.EntityFrameworkCore;
using FluentValidation;

public static class DetailsEndpoints
{
    public static WebApplication MapDetailsEndpoints(this WebApplication app)
    {
        // отправляем детали
        app.MapPost("/api/factory/details", (CreateDetailDto dto, AppDbContext db, ClaimsPrincipal userPrincipal, IValidator<CreateDetailDto> validator) =>
        {
            
            // Добавляем проверку до того как лезим в бд 
            var validationResult = validator.Validate(dto);

            // если результат НЕ ВАЛИДНЫЙ выдаём ошибку
            if (!validationResult.IsValid)
            {
                return Results.BadRequest(validationResult.Errors);
            }
    
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
        app.MapPut("/api/factory/details/{id}", (int id, CreateDetailDto dto, AppDbContext db,  ClaimsPrincipal userPrincipal, IValidator<CreateDetailDto> validator) =>
        {
            
            // Добавляем проверку до того как лезим в бд
            var validationResult = validator.Validate(dto);

            // если результат НЕ ВАЛИДНЫЙ выдаём ошибку
            if (!validationResult.IsValid)
            {
                return Results.BadRequest(validationResult.Errors);
            }
    
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

        // Удаляем детали
        app.MapDelete("/api/factory/details/{id}", (int id, AppDbContext db, ClaimsPrincipal userPrincipal) =>
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

                db.Details.Remove(detail);
                db.SaveChanges();
                return Results.Ok(detail);
            })
            .RequireAuthorization();
        return app;
    }
}
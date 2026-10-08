using FactorySystem.Services;

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
        app.MapPost("/api/factory/details", (DetailService detailService, CreateDetailDto dto,  ClaimsPrincipal principal) =>
            {
                var postDetail = detailService.CreateDetail(dto, principal);
                
                return Results.Ok(postDetail);
            })
        .RequireAuthorization();

        // получаем детали
        app.MapGet("/api/factory/details/", (DetailService detailService) =>
            {
                var getDetailAll = detailService.GetDetailsAll();
                return Results.Ok(getDetailAll);
            })
        .RequireAuthorization();
        
        // Получаем одну конкретную делать
        app.MapGet("/api/factory/details/{id}", (DetailService detailService, int id) =>
        {
            var getDetail = detailService.GetDetail(id);

            if (getDetail != null)
            {
                return Results.Ok(getDetail);
            }
            else
            {
                return Results.NotFound();
            }
        })
        .RequireAuthorization();

        // обновляем детали
        app.MapPut("/api/factory/details/{id}", (DetailService detailService, CreateDetailDto dto, ClaimsPrincipal principal, int id) =>
        {
            var putDetail = detailService.UpdateDetail(dto, principal, id);

            if (putDetail != null)
            {
                return Results.Ok(putDetail);
            }
            else
            {
                return Results.NotFound();
            }
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
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
        .RequireAuthorization("Boss");
        
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
        .RequireAuthorization("Boss");

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
        .RequireAuthorization("Boss");

        // Удаляем детали
        app.MapDelete("/api/factory/details/{id}", (DetailService detailService, int id, ClaimsPrincipal principal) =>
            {
                var deleteDetail = detailService.DeleteDetail(id, principal);

                if (deleteDetail)
                {
                    return Results.Ok(deleteDetail);
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
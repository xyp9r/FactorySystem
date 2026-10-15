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

            return putDetail switch
            {
                { IsSuccess: true }  => Results.Ok(putDetail.Value),
                { Error.Type: ErrorType.NotFound } => Results.NotFound(putDetail.Error.Message),
                { Error.Type: ErrorType.Forbidden } => Results.Forbid(),
                { Error.Type: ErrorType.Validator } => Results.BadRequest(),
                { Error.Type: ErrorType.Failure } => Results.StatusCode(500),
                { Error.Type: ErrorType.Unauthorized } => Results.Unauthorized(),
                _ => Results.StatusCode(500)
            };
        })
        .RequireAuthorization("Boss");

        // Удаляем детали
        app.MapDelete("/api/factory/details/{id}", (DetailService detailService, int id, ClaimsPrincipal principal) =>
            {
                var deleteDetail = detailService.DeleteDetail(id, principal);

                return deleteDetail switch
                {
                    { IsSuccess: true } => Results.NoContent(),
                    { Error.Type: ErrorType.NotFound } => Results.NotFound(deleteDetail.Error.Message),
                    { Error.Type: ErrorType.Forbidden } => Results.Forbid(),
                    { Error.Type: ErrorType.Unauthorized } => Results.Unauthorized(),
                    _ => Results.StatusCode(500)
                };
            })
            .RequireAuthorization("Boss");
        return app;
    }
}
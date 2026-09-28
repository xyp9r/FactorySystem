using Microsoft.AspNetCore.Http;
using System;
using System.Threading.Tasks;
using System.Text.Json;

namespace FactorySystem.Middleware;

public class ExceptionHandlingMiddleware
{
    private readonly RequestDelegate _next; // Ссылка на следующий шаг в трубе

    public ExceptionHandlingMiddleware(RequestDelegate next)
    {
        _next = next;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        try
        {
            await _next(context);
        }
        catch (Exception ex)
        {
            // Присваиваем ошибке статус 500
            context.Response.StatusCode = 500;
            
            // присваиваем json
            context.Response.ContentType = "application/json";
            
            // Упаковываем ошибку
            var result = JsonSerializer.Serialize(new { error = ex.Message });
            
            // Отправляем
            await context.Response.WriteAsync(result);

        }
    }
}
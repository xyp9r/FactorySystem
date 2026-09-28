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
            // TODO: сделать красивый JSON
        }
    }
}
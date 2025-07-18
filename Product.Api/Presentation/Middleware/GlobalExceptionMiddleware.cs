using System.ComponentModel.DataAnnotations;
using System.Text.Json;
using Microsoft.AspNetCore.Http;

namespace Product.Api.Presentation.Middleware;

public class GlobalExceptionMiddleware(RequestDelegate next, ILogger<GlobalExceptionMiddleware> logger)
{
    public async Task InvokeAsync(HttpContext context)
    {
        try
        {
            await next(context);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, $"Unhandled exception: {ex.Message}");
            HandleExceptionAsync(context, ex);
        }
    }

    private static void HandleExceptionAsync(HttpContext context, Exception exception)
    {
        context.Response.ContentType = "application/json";

        var statusCode = exception switch
        {
            KeyNotFoundException => StatusCodes.Status404NotFound,
            ValidationException => StatusCodes.Status400BadRequest,
            _ => StatusCodes.Status500InternalServerError
        };

        var errorResponse = new
        {
            StatusCode = statusCode,
            Message = exception.Message,
            Detail = exception.InnerException?.Message
        };

        context.Response.StatusCode = statusCode;
        var json = JsonSerializer.Serialize(errorResponse); 
        context.Response.WriteAsync(json);
    }
}

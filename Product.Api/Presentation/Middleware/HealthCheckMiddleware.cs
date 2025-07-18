using Microsoft.AspNetCore.Http;
namespace Product.Api.Presentation.Middleware;

public class HealthCheckMiddleware(RequestDelegate next)
{
    public async Task InvokeAsync(HttpContext context)
    {
        if (context.Request.Path.StartsWithSegments("/health"))
        {
            context.Response.StatusCode = 200;
            context.Response.ContentType = "application/json";
            await context.Response.WriteAsync("{\"status\": \"Healthy\"}");
            return;
        }

        await next(context);
    }
}

using Microsoft.AspNetCore.Http;

namespace Product.Api.Presentation.Middleware;

public class RequestResponseLoggingMiddleware(RequestDelegate next, ILogger<RequestResponseLoggingMiddleware> logger)
{
    public async Task Invoke(HttpContext context)
    {
        context.Request.EnableBuffering();

        var requestBodyStream = new MemoryStream();
        await context.Request.Body.CopyToAsync(requestBodyStream);
        requestBodyStream.Seek(0, SeekOrigin.Begin);

        var requestBodyText = await new StreamReader(requestBodyStream).ReadToEndAsync();
        context.Request.Body.Position = 0;

        logger.LogInformation("HTTP Request: {Method} {Path} - Body: {Body}",
            context.Request.Method,
            context.Request.Path,
            requestBodyText);
        
        var originalBodyStream = context.Response.Body;
        using var responseBody = new MemoryStream();
        context.Response.Body = responseBody;

        await next(context);

        context.Response.Body.Seek(0, SeekOrigin.Begin);
        var responseBodyText = await new StreamReader(context.Response.Body).ReadToEndAsync();
        context.Response.Body.Seek(0, SeekOrigin.Begin);

        logger.LogInformation("HTTP Response: {StatusCode} - Body: {Body}",
            context.Response.StatusCode,
            responseBodyText);

        await responseBody.CopyToAsync(originalBodyStream);
    }
}
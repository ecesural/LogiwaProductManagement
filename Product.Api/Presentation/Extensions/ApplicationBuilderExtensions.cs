﻿using Microsoft.AspNetCore.Builder;
using Product.Api.Presentation.Middleware;

namespace Product.Api.Presentation.Extensions;

public static class ApplicationBuilderExtensions
{
    public static IApplicationBuilder UseCustomExceptionHandling(this IApplicationBuilder app)
    {
        return app.UseMiddleware<GlobalExceptionMiddleware>();
    }
    
}
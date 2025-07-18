using FluentValidation;
using FluentValidation.AspNetCore;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Mvc;
using Product.Api.Application.Features.Products.Validators;
using Product.Api.Presentation.Extensions;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddApplicationServices(builder.Configuration);
builder.Services.AddApiVersioning(options =>
{
    options.AssumeDefaultVersionWhenUnspecified = true;
    options.DefaultApiVersion = new ApiVersion(1, 0);
    options.ReportApiVersions = true;
});

builder.Services.AddControllers()
    .AddFluentValidation(fv =>
    {
        fv.RegisterValidatorsFromAssemblyContaining<CreateProductCommandValidator>();
    });

builder.Services.AddValidatorsFromAssemblyContaining<Program>(); 
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new Microsoft.OpenApi.Models.OpenApiInfo
    {
        Title = "Logiwa.Product.Management.Api",
        Version = "v3"
    });
});

var app = builder.Build();

app.UseSwagger();
app.UseSwaggerUI(c =>
{
    c.SwaggerEndpoint("/swagger/v1/swagger.json", "Logiwa.Product.Management.Api v1");
    c.RoutePrefix = string.Empty;
});

app.UseCustomExceptionHandling();
app.UseHttpsRedirection();
app.UseAuthorization();
app.UseRequestResponseLogging();
app.UseCustomHealthCheck();
app.UseRouting();
app.MapControllers();

app.Run();
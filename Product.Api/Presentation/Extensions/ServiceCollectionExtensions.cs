using MediatR;
using Microsoft.EntityFrameworkCore;
using Product.Api.Infrastructure.Caching;
using Product.Api.Infrastructure.Events;
using Product.Api.Infrastructure.Logging;
using Product.Api.Persistence.DbContexts;
using StackExchange.Redis;
using FluentValidation;
using Product.Api.Application.Behaviors;
using Product.Api.Application.Common.Interfaces;
using Product.Api.Application.Features.Products.Commands;
using Product.Api.Application.Features.Products.Services;
using Product.Api.Persistence.Repositories;

namespace Product.Api.Presentation.Extensions;
public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddApplicationServices(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddMediatR(typeof(CreateProductCommand).Assembly);

        services.AddValidatorsFromAssembly(typeof(CreateProductCommand).Assembly);

        services.AddScoped(typeof(ILoggerService<>), typeof(LoggerService<>));
        services.AddTransient(typeof(IPipelineBehavior<,>), typeof(ValidationBehavior<,>));
       
        services.AddSingleton<IConnectionMultiplexer>(sp =>
        {
            var config = $"{configuration["Redis:Host"]}:{configuration["Redis:Port"]},abortConnect=false";
            return ConnectionMultiplexer.Connect(config);
        });
        services.AddScoped<IRedisCacheService, RedisCacheService>();
        services.AddScoped<ICategoryService, CategoryService>();
        
        services.AddScoped<IDomainEventPublisher, DomainEventPublisher>();
        
        services.AddDbContext<ProductDbContext>(options =>
            options.UseSqlServer(configuration.GetConnectionString("DefaultConnection")));

        services.AddScoped<IProductRepository, ProductRepository>();
        services.AddScoped<ICategoryRepository, CategoryRepository>();
        
        return services;
    }
}
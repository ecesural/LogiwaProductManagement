using MediatR;
using Product.Api.Application.Common;
using Product.Api.Application.Common.Interfaces;
using Product.Api.Application.Features.Products.Dtos;
using Product.Api.Application.Features.Products.Queries;
using Product.Api.Presentation.Extensions;

namespace Product.Api.Application.Features.Products.Handlers;

public class FilterProductsQueryHandler(
    IProductRepository productRepository,
    IRedisCacheService redisCacheService,
    ILoggerService<FilterProductsQueryHandler> logger)
    : IRequestHandler<FilterProductsQuery, List<ProductDto>>
{
    public async Task<List<ProductDto>> Handle(FilterProductsQuery request, CancellationToken cancellationToken)
    {
        var cacheKey = GenerateCacheKey(request);
        try
        {
            var (result, fromCache) = await redisCacheService.TryGetOrSetAsync(
                cacheKey,
                async ct =>
                {
                    var products = await productRepository.FilterAsync(
                        request.Keyword,
                        request.MinStock,
                        request.MaxStock,
                        ct);

                    return products.Select(p => new ProductDto
                    {
                        Id = p.Id,
                        Title = p.Title,
                        Description = p.Description,
                        Category = p.Category,
                        StockQuantity = p.StockQuantity,
                        IsLive = p.IsLive
                    }).ToList();
                },
                TimeSpan.FromHours(1),
                cancellationToken: cancellationToken,
                cacheIfEmpty: false
            );

            logger.LogInfo($"Filtered products returned from {(fromCache ? "cache" : "DB")}.");

            return result;
        }
        catch (Exception ex)
        {
            logger.LogError("Error occurred while filtering products.", ex);
            throw;
        }
    }

    private static string GenerateCacheKey(FilterProductsQuery request)
    {
        var keyword = string.IsNullOrWhiteSpace(request.Keyword)
            ? "all"
            : new string(request.Keyword
                    .Where(c => !char.IsWhiteSpace(c))
                    .ToArray())
                .ToLowerInvariant();
        return
            $"{CacheKeys.ProductFilter}k={keyword}&min={request.MinStock?.ToString() ?? "null"}&max={request.MaxStock?.ToString() ?? "null"}";
    }
}
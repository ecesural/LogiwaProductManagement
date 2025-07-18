using MediatR;
using Product.Api.Application.Common.Interfaces;
using Product.Api.Application.Features.Products.Dtos;
using Product.Api.Application.Features.Products.Queries;

namespace Product.Api.Application.Features.Products.Handlers;
public class FilterProductsQueryHandler(
    IProductRepository productRepository,
    IRedisCacheService redisCacheService,
    ILoggerService<FilterProductsQueryHandler> logger)
    : IRequestHandler<FilterProductsQuery, List<ProductDto>>
{
    private const string FilterCachePrefix = "products:filter:";
    public async Task<List<ProductDto>> Handle(FilterProductsQuery request, CancellationToken cancellationToken)
    {
        var cacheKey = GenerateCacheKey(request);
        try
        {
            var cached = await redisCacheService.GetAsync<List<ProductDto>>(cacheKey);
            if (cached is not null)
            {
                logger.LogInfo("Returned product list from cache.");
                return cached;
            }

            var products = await productRepository.FilterAsync(request.Keyword, request.MinStock, request.MaxStock);

            var result = products.Select(p => new ProductDto
            {
                Id = p.Id,
                Title = p.Title,
                Description = p.Description,
                Category = p.Category,
                StockQuantity = p.StockQuantity,
                IsLive = p.IsLive
            }).ToList();
            
            await redisCacheService.SetAsync(cacheKey, result, TimeSpan.FromMinutes(30));
            logger.LogInfo("Product list cached and returned.");

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
        var keywordPart = string.IsNullOrWhiteSpace(request.Keyword)
            ? "all"
            : new string(request.Keyword
                    .Where(c => !char.IsWhiteSpace(c))
                    .ToArray())
                .ToLowerInvariant();
        var minStockPart = request.MinStock.HasValue ? request.MinStock.Value.ToString() : "minnull";
        var maxStockPart = request.MaxStock.HasValue ? request.MaxStock.Value.ToString() : "maxnull";

        return $"{FilterCachePrefix}{keywordPart}:{minStockPart}:{maxStockPart}";
    }
}
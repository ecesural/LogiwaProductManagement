using MediatR;
using Product.Api.Application.Common.Interfaces;
using Product.Api.Application.Features.Products.Dtos;
using Product.Api.Application.Features.Products.Queries;

namespace Product.Api.Application.Features.Products.Handlers;

public class GetAllProductsQueryHandler(
    IProductRepository productRepository,
    IRedisCacheService redisCacheService,
    ILoggerService<GetAllProductsQueryHandler> logger)
    : IRequestHandler<GetAllProductsQuery, List<ProductDto>>
{
    private const string CacheKey = "product:all";

    public async Task<List<ProductDto>> Handle(GetAllProductsQuery request, CancellationToken cancellationToken)
    {
        try
        {
            var cachedProducts = await redisCacheService.GetAsync<List<ProductDto>>(CacheKey);
            if (cachedProducts is not null)
            {
                logger.LogInfo("All products returned from cache.");
                return cachedProducts;
            }

            var products = await productRepository.GetAllAsync();

            var result = products.Select(product => new ProductDto
            {
                Id = product.Id,
                Title = product.Title,
                Description = product.Description,
                Category = product.Category,
                StockQuantity = product.StockQuantity,
                IsLive = product.IsLive
            }).ToList();

            await redisCacheService.SetAsync(CacheKey, result, TimeSpan.FromHours(1));

            logger.LogInfo("All products cached and returned.");
            return result;
        }
        catch (Exception ex)
        {
            logger.LogError("Unexpected error occurred while getting all products.", ex);
            throw;
        }
    }
}
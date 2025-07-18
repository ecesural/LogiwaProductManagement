using MediatR;
using Product.Api.Application.Common.Interfaces;
using Product.Api.Application.Common.Messages;
using Product.Api.Application.Features.Products.Dtos;
using Product.Api.Application.Features.Products.Queries;

namespace Product.Api.Application.Features.Products.Handlers;

public class GetProductByIdQueryHandler(
    IProductRepository productRepository,
    IRedisCacheService redisCacheService,
    ILoggerService<GetProductByIdQueryHandler> logger)
    : IRequestHandler<GetProductByIdQuery, ProductDto>
{
    private const string CachePrefix = "product:";
    public async Task<ProductDto> Handle(GetProductByIdQuery request, CancellationToken cancellationToken)
    {
        var cacheKey = CachePrefix + request.Id;
        try
        {
            var cached = await redisCacheService.GetAsync<ProductDto>(cacheKey);
            if (cached is not null)
            {
                logger.LogInfo($"Product {request.Id} returned from cache.");
                return cached;
            }

            var product = await productRepository.GetByIdAsync(request.Id);
            if (product is null)
            {
                logger.LogWarning($"{ExceptionMessages.ProductNotFound} : {request.Id}");
                throw new KeyNotFoundException(ExceptionMessages.ProductNotFound);
            }

            var result = new ProductDto
            {
                Id = product.Id,
                Title = product.Title,
                Description = product.Description,
                Category = product.Category,
                StockQuantity = product.StockQuantity,
                IsLive = product.IsLive
            };

            await redisCacheService.SetAsync(cacheKey, result, TimeSpan.FromHours(1));

            logger.LogInfo($"Product {request.Id} cached and returned.");
            return result;
        }
        catch (KeyNotFoundException ex)
        {
            logger.LogWarning($"Not found error: {ex.Message}");
            throw; 
        }
        catch (Exception ex)
        {
            logger.LogError($"Unexpected error while getting product {request.Id}.", ex);
            throw; 
        }
    }
}

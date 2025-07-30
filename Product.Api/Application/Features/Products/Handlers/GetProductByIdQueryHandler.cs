using MediatR;
using Product.Api.Application.Common;
using Product.Api.Application.Common.Interfaces;
using Product.Api.Application.Common.Messages;
using Product.Api.Application.Features.Products.Dtos;
using Product.Api.Application.Features.Products.Queries;
using Product.Api.Presentation.Extensions;

namespace Product.Api.Application.Features.Products.Handlers;

public class GetProductByIdQueryHandler(
    IProductRepository productRepository,
    IRedisCacheService redisCacheService,
    ILoggerService<GetProductByIdQueryHandler> logger)
    : IRequestHandler<GetProductByIdQuery, ProductDto>
{
    public async Task<ProductDto> Handle(GetProductByIdQuery request, CancellationToken cancellationToken)
    {
        try
        {
            var (productDto, fromCache) = await redisCacheService.TryGetOrSetAsync(
                CacheKeys.ProductById(request.Id),
                async ct =>
                {
                    var product = await productRepository.GetByIdAsync(request.Id, ct);
                    if (product is not null)
                        return new ProductDto
                        {
                            Id = product.Id,
                            Title = product.Title,
                            Description = product.Description,
                            Category = product.Category,
                            StockQuantity = product.StockQuantity,
                            IsLive = product.IsLive
                        };
                   
                    logger.LogWarning($"Product not found. Id: {request.Id}");
                    throw new KeyNotFoundException(ExceptionMessages.ProductNotFound);
                },
                TimeSpan.FromHours(1),
                cancellationToken: cancellationToken
            );

            logger.LogInfo($"Product {request.Id} returned from {(fromCache ? "cache" : "DB")}.");
            return productDto!;
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

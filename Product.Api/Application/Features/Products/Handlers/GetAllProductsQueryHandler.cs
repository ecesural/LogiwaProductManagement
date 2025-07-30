using MediatR;
using Product.Api.Application.Common;
using Product.Api.Application.Common.Interfaces;
using Product.Api.Application.Features.Products.Dtos;
using Product.Api.Application.Features.Products.Queries;
using Product.Api.Presentation.Extensions;

namespace Product.Api.Application.Features.Products.Handlers;

public class GetAllProductsQueryHandler(
    IProductRepository productRepository,
    IRedisCacheService redisCacheService,
    ILoggerService<GetAllProductsQueryHandler> logger)
    : IRequestHandler<GetAllProductsQuery, List<ProductDto>>
{
    public async Task<List<ProductDto>> Handle(GetAllProductsQuery request, CancellationToken cancellationToken)
    {
        try
        {
            var (productDtos, fromCache) = await redisCacheService.TryGetOrSetAsync(
                CacheKeys.ProductAll,
                async ct =>
                {
                    var products = await productRepository.GetAllAsync(ct);

                    return products.Select(product => new ProductDto
                    {
                        Id = product.Id,
                        Title = product.Title,
                        Description = product.Description,
                        Category = product.Category,
                        StockQuantity = product.StockQuantity,
                        IsLive = product.IsLive
                    }).ToList();
                },
                TimeSpan.FromHours(1),
                cancellationToken: cancellationToken
            );

            logger.LogInfo($"All products returned from {(fromCache ? "cache" : "DB")}.");
            return productDtos;
        }
        catch (Exception ex)
        {
            logger.LogError("Unexpected error occurred while getting all products.", ex);
            throw;
        }
    }
}
using MediatR;
using Product.Api.Application.Common.Interfaces;
using Product.Api.Application.Common.Messages;
using Product.Api.Application.Features.Products.Commands;
using Product.Api.Application.Features.Products.Dtos;
using Product.Api.Application.Features.Products.Responses;

namespace Product.Api.Application.Features.Products.Handlers;

public class CreateProductCommandHandler(
    IProductRepository productRepository,
    IDomainEventPublisher domainEventPublisher,
    ICategoryService categoryService,
    IRedisCacheService redisCacheService,
    ILoggerService<CreateProductCommandHandler> logger)
    : IRequestHandler<CreateProductCommand, CreateAndUpdateProductResponse>
{
    private const string CachePrefixAll = "product:all";
    public async Task<CreateAndUpdateProductResponse> Handle(CreateProductCommand request, CancellationToken cancellationToken)
    {
        logger.LogInfo("CreateProductCommand started.");
        try
        {
            var category = await categoryService.GetCategoryAsync(request.CategoryId);

            var product = new Domain.Entities.Product(
                title: request.Title,
                description: request.Description,
                categoryId: request.CategoryId ?? Guid.Empty,
                category: category,
                stockQuantity: request.StockQuantity
            );

            await productRepository.AddAsync(product);
           
            foreach (var domainEvent in product.DomainEvents)
            {
                await domainEventPublisher.PublishAsync(domainEvent, cancellationToken);
            }
            
            await redisCacheService.RemoveAsync(CachePrefixAll);
           
            logger.LogInfo($"Product created with ID: {product.Id}");
            
            return new CreateAndUpdateProductResponse
            {
                ProductDto = new ProductDto
                {
                    Id = product.Id,
                    Title = product.Title,
                    Description = product.Description,
                    Category = product.Category,
                    StockQuantity = product.StockQuantity,
                    IsLive = product.IsLive
                },
                Message = ResponseMessages.CreatedSuccess
            };
        }
        catch (KeyNotFoundException ex)
        {
            logger.LogWarning($"Not found error: {ex.Message}");
            throw; 
        }
        catch (Exception ex)
        {
            logger.LogError("Unexpected error occurred while creating product.",ex);
            throw;
        }
    }
}
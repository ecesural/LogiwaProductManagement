using MediatR;
using Product.Api.Application.Common.Interfaces;
using Product.Api.Application.Common.Messages;
using Product.Api.Application.Features.Products.Commands;
using Product.Api.Application.Features.Products.Dtos;
using Product.Api.Application.Features.Products.Responses;
using Product.Api.Domain.Entities;
using Product.Api.Presentation.Extensions;

namespace Product.Api.Application.Features.Products.Handlers;

public class UpdateProductCommandHandler(
    IProductRepository productRepository,
    IDomainEventPublisher domainEventPublisher,
    IRedisCacheService redisCacheService,
    ICategoryService categoryService,
    ILoggerService<UpdateProductCommandHandler> logger)
    : IRequestHandler<UpdateProductCommand, CreateAndUpdateProductResponse>
{
    public async Task<CreateAndUpdateProductResponse> Handle(UpdateProductCommand request, CancellationToken cancellationToken)
    {
        logger.LogInfo($"UpdateProductCommand started for ID: {request.ProductId}");
       
        try
        {
            var product = await productRepository.GetByIdAsync(request.ProductId, cancellationToken); 
            if (product is null) 
            { 
                logger.LogWarning($"{ExceptionMessages.ProductNotFound} : {request.ProductId}"); 
                throw new KeyNotFoundException(ExceptionMessages.ProductNotFound);
            }
            
            var (title, description, stockQuantity, category) = await GetUpdatedProductFieldsAsync(request, product, cancellationToken);

            product.Update(title, description, category?.Id, stockQuantity, category);

            await productRepository.UpdateAsync(product,cancellationToken);

            await domainEventPublisher.PublishDomainEventsAsync(product.DomainEvents, logger,
                cancellationToken);
         
            product.ClearDomainEvents(); 
            await redisCacheService.RemoveProductCacheAsync(product.Id, logger, cancellationToken);
         
            logger.LogInfo($"Product updated: {request.ProductId}");
            
            return new CreateAndUpdateProductResponse{
                ProductDto = new ProductDto
                {
                    Id = product.Id,
                    Title = product.Title,
                    Description = product.Description,
                    Category = product.Category,
                    StockQuantity = product.StockQuantity,
                    IsLive = product.IsLive
                },
                Message = ResponseMessages.UpdatedSuccess
            };
        }
        catch (KeyNotFoundException ex)
        {
            logger.LogWarning($"Not found: {ex.Message}");
            throw;
        }
        catch (Exception ex)
        {
            logger.LogError("Unexpected error occurred while updating product.", ex);
            throw;
        }
    }
    
    private async Task<(string title, string? description, int stockQuantity, Category? category)> GetUpdatedProductFieldsAsync(UpdateProductCommand request, Domain.Entities.Product product, CancellationToken cancellationToken)
    {
        var category = request.CategoryId.IsSet
            ? await categoryService.GetCategoryAsync(request.CategoryId.Value, cancellationToken)
            : product.Category;

        var title = request.Title.IsSet
            ? request.Title.Value!
            : product.Title;

        var description = request.Description.IsSet
            ? request.Description.Value
            : product.Description;

        var stockQuantity = request.StockQuantity.IsSet
            ? request.StockQuantity.Value
            : product.StockQuantity;

        return (title, description, stockQuantity, category);
    }
}
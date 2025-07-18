using MediatR;
using Product.Api.Application.Common.Interfaces;
using Product.Api.Application.Common.Messages;
using Product.Api.Application.Features.Products.Commands;
using Product.Api.Application.Features.Products.Responses;

namespace Product.Api.Application.Features.Products.Handlers;
public class DeleteProductCommandHandler(
    IProductRepository productRepository,
    IDomainEventPublisher domainEventPublisher,
    IRedisCacheService redisCacheService,
    ILoggerService<DeleteProductCommandHandler> logger)
    :  IRequestHandler<DeleteProductCommand, DeleteProductResponse>
{
    private const string CachePrefix = "product:";
    private const string CachePrefixAll = "product:all";

    public async Task<DeleteProductResponse> Handle(DeleteProductCommand request, CancellationToken cancellationToken)
    {
        logger.LogInfo($"DeleteProductCommand started for ID: {request.Id}");

        try
        {
            var product = await productRepository.GetByIdAsync(request.Id);
            if (product == null)
            {
                logger.LogWarning($"{ExceptionMessages.ProductNotFound} : {request.Id}");
                throw new KeyNotFoundException(ExceptionMessages.ProductNotFound);
            }

            product.Delete();

            await productRepository.DeleteAsync(product);
            foreach (var domainEvent in product.DomainEvents)
            {
                await domainEventPublisher.PublishAsync(domainEvent, cancellationToken);
            }
        
            var cacheKey = CachePrefix + product.Id;
            await redisCacheService.RemoveAsync(cacheKey);
            await redisCacheService.RemoveAsync(CachePrefixAll);

            logger.LogInfo($"Product deleted: {request.Id}");

            return new DeleteProductResponse
            {
                ProductId = request.Id,
                Message = ResponseMessages.DeletedSuccess
            };
        }
        catch (KeyNotFoundException ex)
        {
            logger.LogWarning(ex.Message);
            throw;
        }
        catch (Exception ex)
        {
            logger.LogError("Unexpected error occurred while deleting product.", ex);
            throw;
        }
    }
}
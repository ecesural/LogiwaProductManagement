using MediatR;
using Product.Api.Application.Common.Interfaces;
using Product.Api.Application.Common.Messages;
using Product.Api.Application.Features.Products.Commands;
using Product.Api.Application.Features.Products.Responses;
using Product.Api.Presentation.Extensions;

namespace Product.Api.Application.Features.Products.Handlers;
public class DeleteProductCommandHandler(
    IProductRepository productRepository,
    IDomainEventPublisher domainEventPublisher,
    IRedisCacheService redisCacheService,
    ILoggerService<DeleteProductCommandHandler> logger)
    :  IRequestHandler<DeleteProductCommand, DeleteProductResponse>
{
    public async Task<DeleteProductResponse> Handle(DeleteProductCommand request, CancellationToken cancellationToken)
    {
        logger.LogInfo($"DeleteProductCommand started for ID: {request.Id}");

        try
        {
            var product = await productRepository.GetByIdAsync(request.Id,cancellationToken);
            if (product == null)
            {
                logger.LogWarning($"{ExceptionMessages.ProductNotFound} : {request.Id}");
                throw new KeyNotFoundException(ExceptionMessages.ProductNotFound);
            }

            product.AddDeleteEvent();
            await productRepository.DeleteAsync(product, cancellationToken);
            await redisCacheService.RemoveProductCacheAsync(product.Id, logger, cancellationToken);
     
            await domainEventPublisher.PublishDomainEventsAsync(product.DomainEvents, logger,
                cancellationToken);
            
            product.ClearDomainEvents(); 
            
            
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
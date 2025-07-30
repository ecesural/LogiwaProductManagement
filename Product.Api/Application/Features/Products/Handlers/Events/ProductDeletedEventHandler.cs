using MediatR;
using Product.Api.Domain.Events;

namespace Product.Api.Application.Features.Products.Handlers.Events;

public class ProductDeletedEventHandler(ILogger<ProductDeletedEventHandler> logger)
    : INotificationHandler<ProductDeletedEvent>
{
    public Task Handle(ProductDeletedEvent notification, CancellationToken cancellationToken)
    {
        logger.LogInformation("Product deleted: {ProductId}", notification.Product.Id);
        return Task.CompletedTask;
    }
}

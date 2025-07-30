using MediatR;
using Product.Api.Domain.Events;

namespace Product.Api.Application.Features.Products.Handlers.Events;

public class ProductUpdatedEventHandler(ILogger<ProductUpdatedEventHandler> logger)
    : INotificationHandler<ProductUpdatedEvent>
{
    public Task Handle(ProductUpdatedEvent notification, CancellationToken cancellationToken)
    {
        logger.LogInformation("Product updated: {ProductId}", notification.Product.Id);
        return Task.CompletedTask;
    }
}

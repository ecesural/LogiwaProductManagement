using MediatR;
using Product.Api.Domain.Events;

namespace Product.Api.Application.Features.Products.Handlers.Events;

public class ProductCreatedEventHandler(ILogger<ProductCreatedEventHandler> logger)
    : INotificationHandler<ProductCreatedEvent>
{
    public Task Handle(ProductCreatedEvent notification, CancellationToken cancellationToken)
    {
        logger.LogInformation("Product created: {ProductId}", notification.Product.Id);
        return Task.CompletedTask;
    }
}

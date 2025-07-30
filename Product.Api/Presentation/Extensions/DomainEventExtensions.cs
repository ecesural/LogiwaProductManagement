using MediatR;
using Product.Api.Application.Common.Interfaces;

namespace Product.Api.Presentation.Extensions;

public static class DomainEventPublisherExtensions
{
    public static async Task PublishDomainEventsAsync<T>(
        this IDomainEventPublisher publisher,
        IEnumerable<INotification> domainEvents,
        ILoggerService<T> logger,
        CancellationToken cancellationToken = default,
        bool throwOnFailure = false)
    {
        var publishTasks = domainEvents.Select(async domainEvent =>
        {
            try
            {
                await publisher.PublishAsync(domainEvent, cancellationToken);
            }
            catch (Exception ex)
            {
                logger.LogError($"Domain event publish failed: {domainEvent.GetType().Name}", ex);
                if (throwOnFailure)
                    throw;
            }
        });
        
        await Task.WhenAll(publishTasks);
    }
}
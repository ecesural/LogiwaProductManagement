using Product.Api.Application.Common.Interfaces;
using MediatR;

namespace Product.Api.Infrastructure.Events;

public class DomainEventPublisher(IMediator mediator) : IDomainEventPublisher
{
    public async Task PublishAsync<TEvent>(TEvent domainEvent, CancellationToken cancellationToken = default)
        where TEvent : INotification
    {
        await mediator.Publish(domainEvent, cancellationToken);
    }
}

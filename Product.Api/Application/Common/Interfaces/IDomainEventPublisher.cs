using MediatR;

namespace Product.Api.Application.Common.Interfaces;

public interface IDomainEventPublisher
{
    Task PublishAsync<TEvent>(TEvent domainEvent, CancellationToken cancellationToken = default)
        where TEvent : INotification;
}
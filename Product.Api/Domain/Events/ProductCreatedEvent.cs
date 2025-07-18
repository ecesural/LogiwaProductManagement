using MediatR;

namespace Product.Api.Domain.Events;

public record ProductCreatedEvent(Entities.Product Product) : INotification;
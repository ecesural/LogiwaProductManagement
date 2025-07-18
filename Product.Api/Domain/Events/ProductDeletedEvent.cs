using MediatR;

namespace Product.Api.Domain.Events;

public record ProductDeletedEvent(Entities.Product Product) : INotification;
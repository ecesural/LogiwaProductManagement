using MediatR;
namespace Product.Api.Domain.Events;

public record ProductUpdatedEvent(Entities.Product Product) : INotification;
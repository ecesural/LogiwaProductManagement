using MediatR;
using Product.Api.Application.Features.Products.Responses;

namespace Product.Api.Application.Features.Products.Commands;

public record DeleteProductCommand(Guid Id) : IRequest<DeleteProductResponse>;
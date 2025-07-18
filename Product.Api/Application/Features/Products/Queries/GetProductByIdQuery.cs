using MediatR;
using Product.Api.Application.Features.Products.Dtos;

namespace Product.Api.Application.Features.Products.Queries;

public record GetProductByIdQuery(Guid Id) : IRequest<ProductDto>;
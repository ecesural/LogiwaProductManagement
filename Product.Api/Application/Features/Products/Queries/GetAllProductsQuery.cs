using MediatR;
using Product.Api.Application.Features.Products.Dtos;

namespace Product.Api.Application.Features.Products.Queries;

public record GetAllProductsQuery : IRequest<List<ProductDto>>;
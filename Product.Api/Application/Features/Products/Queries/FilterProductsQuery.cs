using MediatR;
using Product.Api.Application.Features.Products.Dtos;

namespace Product.Api.Application.Features.Products.Queries;

public record FilterProductsQuery(
    string? Keyword,
    int? MinStock,
    int? MaxStock
) : IRequest<List<ProductDto>>;
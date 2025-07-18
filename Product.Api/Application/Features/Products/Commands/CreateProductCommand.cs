using MediatR;
using Product.Api.Application.Features.Products.Responses;

namespace Product.Api.Application.Features.Products.Commands;

public record CreateProductCommand(
    string Title,
    string? Description,
    Guid? CategoryId,
    int StockQuantity
) : IRequest<CreateAndUpdateProductResponse>;
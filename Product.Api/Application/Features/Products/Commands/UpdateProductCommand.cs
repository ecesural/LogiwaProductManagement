using MediatR;
using Product.Api.Application.Features.Products.Responses;
using Product.Api.Application.Features.Products.Utils;

namespace Product.Api.Application.Features.Products.Commands;

public record UpdateProductCommand(
    Guid ProductId,
    OptionalField<string> Title,
    OptionalField<string?> Description,    
    OptionalField<Guid?> CategoryId,        
    OptionalField<int> StockQuantity        
) : IRequest<CreateAndUpdateProductResponse>;
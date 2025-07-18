using Product.Api.Application.Features.Products.Dtos;

namespace Product.Api.Application.Features.Products.Responses;

public class CreateAndUpdateProductResponse
{
    public ProductDto ProductDto { get; set; }
    public string Message { get; set; }
}
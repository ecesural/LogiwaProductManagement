using Product.Api.Domain.Entities;

namespace Product.Api.Application.Features.Products.Dtos;

public class ProductDto
{
    public Guid Id { get; set; }
    public string Title { get; set; }
    public string? Description { get; set; }
    public Category? Category { get; set; }
    public int StockQuantity { get; set; }
    public bool IsLive { get; set; }
}
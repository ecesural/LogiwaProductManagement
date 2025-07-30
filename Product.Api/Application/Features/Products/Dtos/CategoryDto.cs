namespace Product.Api.Application.Features.Products.Dtos;

public class CategoryDto
{
    public Guid Id { get; init; }
    public string Name { get; init; } 
    public int MinStockQuantity { get; init; }
}
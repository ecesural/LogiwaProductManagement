namespace Product.Api.Domain.Entities;

public class Category
{
    public Guid Id { get; private set; } = Guid.NewGuid();
    public string Name { get; private set; }
    public int MinStockQuantity { get; private set; }

    private Category() { }

    public Category(string name, int minStockQuantity)
    {
        Name = name;
        MinStockQuantity = minStockQuantity;
    }
}
using System.ComponentModel.DataAnnotations.Schema;
using MediatR; 
using Product.Api.Domain.Events;

namespace Product.Api.Domain.Entities;

public class Product
{
    public Guid Id { get; private set; } = Guid.NewGuid();
    public string Title { get; private set; }
    public string? Description { get; private set; }
    public Guid? CategoryId { get; private set; } 
    public Category? Category { get; private set; }
    public int StockQuantity { get; private set; }
    public bool IsLive { get; private set; }

    [NotMapped] 
    public List<INotification> DomainEvents { get; private set; } = new();

    private Product() { } 

    public Product(string title, string? description, Guid? categoryId, int stockQuantity, Category? category)
    {
        Title = title;
        Description = description;
        CategoryId = categoryId ?? null;
        Category = category;
        StockQuantity = stockQuantity;
        SetLiveStatus();

        DomainEvents.Add(new ProductCreatedEvent(this));
    }

    public void Update(string title, string? description, Guid? categoryId, int stockQuantity, Category? category)
    {
        Title = title;
        Description = description;
        CategoryId = categoryId ?? null;
        Category = category;
        StockQuantity = stockQuantity;
        SetLiveStatus();

        DomainEvents.Add(new ProductUpdatedEvent(this));
    }

    private void SetLiveStatus()
    {
        IsLive = Category is not null &&
                 StockQuantity >= (Category?.MinStockQuantity ?? 0);
    }

    public void AddDeleteEvent()
    {
        DomainEvents.Add(new ProductDeletedEvent(this));
    }
    
    public void ClearDomainEvents()
    {
        DomainEvents.Clear();
    }
}
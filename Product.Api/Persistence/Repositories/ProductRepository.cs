using Microsoft.EntityFrameworkCore;
using Product.Api.Application.Common.Interfaces;
using Product.Api.Persistence.DbContexts;
namespace Product.Api.Persistence.Repositories;

public class ProductRepository(ProductDbContext context) : IProductRepository
{
    public async Task AddAsync(Domain.Entities.Product product, CancellationToken cancellationToken = default)
    {
        if (product.Category is not null)
        {
            context.Attach(product.Category);
        }
        await context.Products.AddAsync(product, cancellationToken);
        await context.SaveChangesAsync(cancellationToken);
    }

    public async Task<Domain.Entities.Product?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await context.Products
            .AsNoTracking()
            .Include(p => p.Category)
            .FirstOrDefaultAsync(p => p.Id == id, cancellationToken);
    }

    public async Task<List<Domain.Entities.Product>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        return await context.Products
            .AsNoTracking()
            .Include(p => p.Category)
            .ToListAsync(cancellationToken);
    }

    public async Task<List<Domain.Entities.Product>> FilterAsync(string? keyword, int? minStock, int? maxStock, CancellationToken cancellationToken = default)
    {
        var query = context.Products
            .Include(p => p.Category).AsQueryable();
        
        if (!string.IsNullOrWhiteSpace(keyword))
        {
            var words = keyword
                .Split(' ', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);

            foreach (var word in words)
            {
                var temp = word;
                query = query.Where(p =>
                    p.Title.Contains(temp) ||
                    p.Description.Contains(temp) ||
                    (p.Category != null && p.Category.Name.Contains(temp)));
            }
        }

        if (minStock.HasValue)
        {
            query = query.Where(p => p.StockQuantity >= minStock.Value);
        }

        if (maxStock.HasValue)
        {
            query = query.Where(p => p.StockQuantity <= maxStock.Value);
        }

        return await query.AsNoTracking().ToListAsync(cancellationToken);
    }

    public async Task UpdateAsync(Domain.Entities.Product product, CancellationToken cancellationToken = default)
    {
        context.Products.Update(product);
        await context.SaveChangesAsync(cancellationToken);
    }

    public async Task DeleteAsync(Domain.Entities.Product product, CancellationToken cancellationToken = default)
    {
        context.Products.Remove(product);
        await context.SaveChangesAsync(cancellationToken);
    }
}


using Microsoft.EntityFrameworkCore;
using Product.Api.Application.Common.Interfaces;
using Product.Api.Persistence.DbContexts;
namespace Product.Api.Persistence.Repositories;

public class ProductRepository(ProductDbContext context) : IProductRepository
{
    public async Task AddAsync(Domain.Entities.Product product)
    {
        await context.Products.AddAsync(product);
        await context.SaveChangesAsync();
    }
    public async Task<Domain.Entities.Product?> GetByIdAsync(Guid id)
    {
        return await context.Products.AsNoTracking().Include(p => p.Category)
            .FirstOrDefaultAsync(p => p.Id == id);
    }
    public async Task<List<Domain.Entities.Product>> GetAllAsync()
    {
        return await context.Products.AsNoTracking()
            .Include(p => p.Category)
            .ToListAsync();
    }
    public async Task<List<Domain.Entities.Product>> FilterAsync(string? keyword, int? minStock, int? maxStock)
    {
        IQueryable<Domain.Entities.Product> query = context.Products.AsNoTracking().Include(p => p.Category);

        if (!string.IsNullOrWhiteSpace(keyword))
        {
            query = query.Where(p =>
                p.Title.Contains(keyword) ||
                p.Description.Contains(keyword) ||
                (p.Category != null && p.Category.Name.Contains(keyword)));
        }

        if (minStock.HasValue)
        {
            query = query.Where(p => p.StockQuantity >= minStock.Value);
        }

        if (maxStock.HasValue)
        {
            query = query.Where(p => p.StockQuantity <= maxStock.Value);
        }

        return await query.ToListAsync();
    }
    public async Task UpdateAsync(Domain.Entities.Product product)
    {
        context.Products.Update(product);
        await context.SaveChangesAsync();
    }
    public async Task DeleteAsync(Domain.Entities.Product product)
    {
        context.Products.Remove(product);
        await context.SaveChangesAsync();
    }
}


namespace Product.Api.Application.Common.Interfaces;

public interface IProductRepository
{
    Task AddAsync(Domain.Entities.Product product);
    Task<List<Domain.Entities.Product>> GetAllAsync();
    Task<Domain.Entities.Product?> GetByIdAsync(Guid id);
    Task<List<Domain.Entities.Product>> FilterAsync(string? keyword, int? minStock, int? maxStock);
    Task UpdateAsync(Domain.Entities.Product product);
    Task DeleteAsync(Domain.Entities.Product product);
}
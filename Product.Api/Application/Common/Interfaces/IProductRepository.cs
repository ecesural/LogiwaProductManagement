namespace Product.Api.Application.Common.Interfaces;

public interface IProductRepository
{
    Task AddAsync(Domain.Entities.Product product, CancellationToken cancellationToken = default);
    Task<List<Domain.Entities.Product>> GetAllAsync(CancellationToken cancellationToken = default);
    Task<Domain.Entities.Product?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<List<Domain.Entities.Product>> FilterAsync(string? keyword, int? minStock, int? maxStock, CancellationToken cancellationToken = default);
    Task UpdateAsync(Domain.Entities.Product product, CancellationToken cancellationToken = default);
    Task DeleteAsync(Domain.Entities.Product product, CancellationToken cancellationToken = default);
}
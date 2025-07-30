using Product.Api.Domain.Entities;

namespace Product.Api.Application.Common.Interfaces;

public interface ICategoryRepository
{
    Task<Category?> GetByIdAsync(Guid? id, CancellationToken cancellationToken = default);
}
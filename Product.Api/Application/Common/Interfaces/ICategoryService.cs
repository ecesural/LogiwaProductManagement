using Product.Api.Domain.Entities;

namespace Product.Api.Application.Common.Interfaces;

public interface ICategoryService
{
    Task<Category?> GetCategoryAsync(Guid? categoryId);
}
using Product.Api.Application.Common.Interfaces;
using Product.Api.Application.Common.Messages;
using Product.Api.Application.Features.Products.Handlers;
using Product.Api.Domain.Entities;

namespace Product.Api.Infrastructure.Services;

public class CategoryService(
    ICategoryRepository categoryRepository,
    ILoggerService<CreateProductCommandHandler> logger) : ICategoryService
{
    public async Task<Category?> GetCategoryAsync(Guid? categoryId, CancellationToken cancellationToken = default)
    {
        if (categoryId is null || categoryId == Guid.Empty)
            return null;

        var category = await categoryRepository.GetByIdAsync(categoryId.Value, cancellationToken);
      
        if (category is not null) return category;
      
        logger.LogWarning($"{ExceptionMessages.CategoryNotFound} : {categoryId}");
        throw new KeyNotFoundException(ExceptionMessages.CategoryNotFound);
    }
}
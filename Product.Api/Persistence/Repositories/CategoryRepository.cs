﻿using Microsoft.EntityFrameworkCore;
using Product.Api.Application.Common.Interfaces;
using Product.Api.Domain.Entities;
using Product.Api.Persistence.DbContexts;

namespace Product.Api.Persistence.Repositories;

public class CategoryRepository(ProductDbContext context) : ICategoryRepository
{
    public async Task<Category?> GetByIdAsync(Guid? id, CancellationToken cancellationToken = default)
    {
        return await context.Categories.AsNoTracking()
            .FirstOrDefaultAsync(p => p.Id == id, cancellationToken);
    }
}


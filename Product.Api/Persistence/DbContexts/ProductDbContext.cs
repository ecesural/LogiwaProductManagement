using Microsoft.EntityFrameworkCore;
using Product.Api.Domain.Entities;

namespace Product.Api.Persistence.DbContexts;

public class ProductDbContext(DbContextOptions<ProductDbContext> options) : DbContext(options)
{
    public DbSet<Domain.Entities.Product> Products => Set<Domain.Entities.Product>();
    public DbSet<Category> Categories => Set<Category>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<Domain.Entities.Product>(entity =>
        {
            entity.HasKey(p => p.Id);
            entity.Property(p => p.Title)
                .HasMaxLength(200)
                .IsRequired();
            entity.Property(p => p.Description)
                .HasMaxLength(1000)
                .IsRequired(false);
            entity.Property(p => p.StockQuantity);
            entity.HasOne(p => p.Category)
                .WithMany()
                .HasForeignKey(p => p.CategoryId);
        });

        modelBuilder.Entity<Category>(entity =>
        {
            entity.HasKey(c => c.Id);
            entity.Property(c => c.Name)
                .IsRequired();
            entity.Property(c => c.MinStockQuantity)
                .IsRequired();
        });
    }
}

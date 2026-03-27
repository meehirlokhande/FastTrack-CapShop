using CapShop.AdminService.Models;
using Microsoft.EntityFrameworkCore;

namespace CapShop.AdminService.Data;

public class AdminCatalogDbContext : DbContext
{
    public AdminCatalogDbContext(DbContextOptions<AdminCatalogDbContext> options) : base(options) { }

    public DbSet<Product> Products => Set<Product>();
    public DbSet<Category> Categories => Set<Category>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Category>(entity =>
        {
            entity.HasKey(x => x.Id);
            entity.Property(x => x.Name).HasMaxLength(100).IsRequired();
        });

        modelBuilder.Entity<Product>(entity =>
        {
            entity.HasKey(x => x.Id);
            entity.Property(x => x.Name).HasMaxLength(200).IsRequired();
            entity.Property(x => x.Price).HasColumnType("decimal(10,2)");
            entity.Property(x => x.DiscountPrice).HasColumnType("decimal(10,2)");
            entity.Property(x => x.ImageUrl).HasMaxLength(500);
            entity.Property(x => x.Status).HasConversion<int>();

            entity.HasOne(x => x.Category)
                .WithMany(x => x.Products)
                .HasForeignKey(x => x.CategoryId);
        });
    }
}
using CapShop.AdminService.Models;
using Microsoft.EntityFrameworkCore;

namespace CapShop.AdminService.Data;

public class AdminOrderDbContext : DbContext
{
    public AdminOrderDbContext(DbContextOptions<AdminOrderDbContext> options) : base(options) { }

    public DbSet<Order> Orders => Set<Order>();
    public DbSet<OrderItem> OrderItems => Set<OrderItem>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Order>(entity =>
        {
            entity.HasKey(x => x.Id);
            entity.Property(x => x.TotalAmount).HasColumnType("decimal(10,2)");
            entity.Property(x => x.Status).HasConversion<int>();
            entity.Property(x => x.ShippingAddress).HasMaxLength(300);
            entity.Property(x => x.ShippingCity).HasMaxLength(100);
            entity.Property(x => x.ShippingPincode).HasMaxLength(10);

            entity.HasMany(x => x.Items)
                .WithOne(x => x.Order)
                .HasForeignKey(x => x.OrderId);
        });

        modelBuilder.Entity<OrderItem>(entity =>
        {
            entity.HasKey(x => x.Id);
            entity.Property(x => x.ProductName).HasMaxLength(200);
            entity.Property(x => x.Price).HasColumnType("decimal(10,2)");
            entity.Property(x => x.ImageUrl).HasMaxLength(500);
        });
    }
}
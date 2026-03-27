using CapShop.AdminService.Models;
using Microsoft.EntityFrameworkCore;

namespace CapShop.AdminService.Data;

public class AdminDbContext : DbContext
{
    public AdminDbContext(DbContextOptions<AdminDbContext> options) : base(options) { }

    public DbSet<AdminLog> AdminLogs => Set<AdminLog>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<AdminLog>(entity =>
        {
            entity.HasKey(x => x.Id);
            entity.HasIndex(x => x.AdminUserId);
            entity.HasIndex(x => x.CreatedAt);
            entity.Property(x => x.Action).HasMaxLength(50).IsRequired();
            entity.Property(x => x.EntityType).HasMaxLength(50).IsRequired();
            entity.Property(x => x.EntityId).HasMaxLength(100).IsRequired();
            entity.Property(x => x.Details).HasMaxLength(500);
        });
    }
}
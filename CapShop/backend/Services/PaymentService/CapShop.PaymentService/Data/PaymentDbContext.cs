using CapShop.PaymentService.Models;
using Microsoft.EntityFrameworkCore;

namespace CapShop.PaymentService.Data;

public class PaymentDbContext : DbContext
{
    public PaymentDbContext(DbContextOptions<PaymentDbContext> options) : base(options) { }

    public DbSet<Transaction> Transactions => Set<Transaction>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Transaction>(entity =>
        {
            entity.HasKey(x => x.Id);
            entity.HasIndex(x => x.OrderId);
            entity.HasIndex(x => x.UserId);
            entity.Property(x => x.Amount).HasColumnType("decimal(10,2)");
            entity.Property(x => x.Method).HasConversion<int>();
            entity.Property(x => x.Status).HasConversion<int>();
        });
    }
}

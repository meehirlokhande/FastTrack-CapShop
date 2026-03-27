using CapShop.AdminService.Data;
using CapShop.AdminService.Models;
using Microsoft.EntityFrameworkCore;

namespace CapShop.AdminService.Repositories;

public class AdminOrderRepository : IAdminOrderRepository
{
    private readonly AdminOrderDbContext _db;

    public AdminOrderRepository(AdminOrderDbContext db)
    {
        _db = db;
    }

    public async Task<List<Order>> GetAllAsync()
    {
        return await _db.Orders
            .Include(o => o.Items)
            .OrderByDescending(o => o.CreatedAt)
            .ToListAsync();
    }

    public async Task<Order?> GetByIdAsync(Guid id)
    {
        return await _db.Orders
            .Include(o => o.Items)
            .FirstOrDefaultAsync(o => o.Id == id);
    }

    public async Task UpdateAsync(Order order)
    {
        _db.Orders.Update(order);
        await _db.SaveChangesAsync();
    }

    public async Task<int> GetTotalCountAsync()
    {
        return await _db.Orders.CountAsync();
    }

    public async Task<decimal> GetTotalRevenueAsync()
    {
        return await _db.Orders
            .Where(o => o.Status == OrderStatus.Paid
                     || o.Status == OrderStatus.Packed
                     || o.Status == OrderStatus.Shipped
                     || o.Status == OrderStatus.Delivered)
            .SumAsync(o => o.TotalAmount);
    }

    public async Task<int> GetCountByStatusAsync(OrderStatus status)
    {
        return await _db.Orders.CountAsync(o => o.Status == status);
    }

    public async Task<List<Order>> GetRecentAsync(int count)
    {
        return await _db.Orders
            .Include(o => o.Items)
            .OrderByDescending(o => o.CreatedAt)
            .Take(count)
            .ToListAsync();
    }

    public async Task<List<Order>> GetByDateRangeAsync(DateTime from, DateTime to)
    {
        return await _db.Orders
            .Where(o => o.CreatedAt >= from && o.CreatedAt <= to)
            .Where(o => o.Status != OrderStatus.PaymentFailed && o.Status != OrderStatus.Cancelled)
            .ToListAsync();
    }
}
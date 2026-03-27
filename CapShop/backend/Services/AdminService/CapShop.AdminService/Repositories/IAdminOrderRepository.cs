using CapShop.AdminService.Models;

namespace CapShop.AdminService.Repositories;

public interface IAdminOrderRepository
{
    Task<List<Order>> GetAllAsync();
    Task<Order?> GetByIdAsync(Guid id);
    Task UpdateAsync(Order order);
    Task<int> GetTotalCountAsync();
    Task<decimal> GetTotalRevenueAsync();
    Task<int> GetCountByStatusAsync(OrderStatus status);
    Task<List<Order>> GetRecentAsync(int count);
    Task<List<Order>> GetByDateRangeAsync(DateTime from, DateTime to);
}
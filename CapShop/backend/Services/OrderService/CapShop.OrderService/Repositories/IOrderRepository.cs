using CapShop.OrderService.Models;

namespace CapShop.OrderService.Repositories;

public interface IOrderRepository
{
    Task<Order?> GetByIdAsync(Guid orderId);
    Task<List<Order>> GetByUserIdAsync(Guid userId);
    Task AddAsync(Order order);
    Task UpdateAsync(Order order);
}
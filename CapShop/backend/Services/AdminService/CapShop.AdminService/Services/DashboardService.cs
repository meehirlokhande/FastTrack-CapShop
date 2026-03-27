using CapShop.AdminService.Dtos;
using CapShop.AdminService.Models;
using CapShop.AdminService.Repositories;

namespace CapShop.AdminService.Services;

public class DashboardService : IDashboardService
{
    private readonly IAdminProductRepository _products;
    private readonly IAdminOrderRepository _orders;

    public DashboardService(IAdminProductRepository products, IAdminOrderRepository orders)
    {
        _products = products;
        _orders = orders;
    }

    public async Task<DashboardSummaryResponse> GetSummaryAsync()
    {
        var recentOrders = await _orders.GetRecentAsync(5);

        return new DashboardSummaryResponse
        {
            TotalProducts = await _products.GetTotalCountAsync(),
            TotalOrders = await _orders.GetTotalCountAsync(),
            TotalRevenue = await _orders.GetTotalRevenueAsync(),
            PendingOrders = await _orders.GetCountByStatusAsync(OrderStatus.Paid),
            DeliveredOrders = await _orders.GetCountByStatusAsync(OrderStatus.Delivered),
            RecentOrders = recentOrders.Select(o => new AdminOrderResponse
            {
                Id = o.Id,
                UserId = o.UserId,
                Status = o.Status.ToString(),
                TotalAmount = o.TotalAmount,
                ShippingCity = o.ShippingCity,
                ItemCount = o.Items.Count,
                CreatedAt = o.CreatedAt,
                PaidAt = o.PaidAt
            }).ToList()
        };
    }
}
using CapShop.AdminService.Dtos;

namespace CapShop.AdminService.Services;

public interface IOrderStatusService
{
    Task<List<AdminOrderResponse>> GetAllOrdersAsync();
    Task<AdminOrderResponse> UpdateOrderStatusAsync(Guid adminUserId, Guid orderId, UpdateOrderStatusRequest request);
}
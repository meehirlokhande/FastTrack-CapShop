using CapShop.OrderService.Dtos;

namespace CapShop.OrderService.Services;

public interface IOrderManagementService
{
    Task<OrderResponse> CheckoutAsync(Guid userId, CheckoutRequest request);
    Task UpdatePaymentStatusAsync(Guid orderId, Guid userId, string status, string paymentMethod, DateTime? paidAt);
    Task<List<OrderListResponse>> GetMyOrdersAsync(Guid userId);
    Task<OrderResponse> GetOrderByIdAsync(Guid userId, Guid orderId);
    Task CancelOrderAsync(Guid userId, Guid orderId);
}
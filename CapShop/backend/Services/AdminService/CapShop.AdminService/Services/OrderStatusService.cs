using CapShop.AdminService.Dtos;
using CapShop.AdminService.Models;
using CapShop.AdminService.Repositories;

namespace CapShop.AdminService.Services;

public class OrderStatusService : IOrderStatusService
{
    private readonly IAdminOrderRepository _orders;
    private readonly IAdminLogService _log;
    private readonly ILogger<OrderStatusService> _logger;

    public OrderStatusService(IAdminOrderRepository orders, IAdminLogService log, ILogger<OrderStatusService> logger)
    {
        _orders = orders;
        _log = log;
        _logger = logger;
    }

    public async Task<List<AdminOrderResponse>> GetAllOrdersAsync()
    {
        var orders = await _orders.GetAllAsync();
        return orders.Select(MapToResponse).ToList();
    }

    public async Task<AdminOrderResponse> UpdateOrderStatusAsync(Guid adminUserId, Guid orderId, UpdateOrderStatusRequest request)
    {
        var order = await _orders.GetByIdAsync(orderId)
            ?? throw new KeyNotFoundException("Order not found.");

        if (!Enum.TryParse<OrderStatus>(request.Status, true, out var newStatus))
            throw new ArgumentException("Invalid status. Use: Paid, Packed, Shipped, Delivered, Cancelled.");

        var allowed = GetAllowedTransitions(order.Status);
        if (!allowed.Contains(newStatus))
            throw new InvalidOperationException($"Cannot transition from {order.Status} to {newStatus}.");

        var oldStatus = order.Status;
        order.Status = newStatus;
        await _orders.UpdateAsync(order);

        await _log.LogAsync(adminUserId, "StatusChanged", "Order", orderId.ToString(),
            $"Status changed from {oldStatus} to {newStatus}");

        _logger.LogInformation("Order {OrderId} status: {Old} -> {New}", orderId, oldStatus, newStatus);
        return MapToResponse(order);
    }

    private static List<OrderStatus> GetAllowedTransitions(OrderStatus current)
    {
        return current switch
        {
            OrderStatus.Paid => new List<OrderStatus> { OrderStatus.Packed, OrderStatus.Cancelled },
            OrderStatus.Packed => new List<OrderStatus> { OrderStatus.Shipped, OrderStatus.Cancelled },
            OrderStatus.Shipped => new List<OrderStatus> { OrderStatus.Delivered },
            _ => new List<OrderStatus>()
        };
    }

    private static AdminOrderResponse MapToResponse(Order o)
    {
        return new AdminOrderResponse
        {
            Id = o.Id,
            UserId = o.UserId,
            Status = o.Status.ToString(),
            TotalAmount = o.TotalAmount,
            ShippingCity = o.ShippingCity,
            ItemCount = o.Items.Count,
            CreatedAt = o.CreatedAt,
            PaidAt = o.PaidAt
        };
    }
}
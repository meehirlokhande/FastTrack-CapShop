using CapShop.OrderService.Dtos;
using CapShop.OrderService.Models;
using CapShop.OrderService.Repositories;
using CapShop.Shared.Events;
using CapShop.Shared.Messaging;
using CapShop.Shared.Middleware;
using Microsoft.AspNetCore.Http;

namespace CapShop.OrderService.Services;

public class OrderManagementService : IOrderManagementService
{
    private readonly IOrderRepository _orders;
    private readonly ICartRepository _carts;
    private readonly IEventPublisher _events;
    private readonly ICatalogHttpClient _catalog;
    private readonly IHttpContextAccessor _httpContextAccessor;
    private readonly ILogger<OrderManagementService> _logger;

    public OrderManagementService(
        IOrderRepository orders,
        ICartRepository carts,
        IEventPublisher events,
        ICatalogHttpClient catalog,
        IHttpContextAccessor httpContextAccessor,
        ILogger<OrderManagementService> logger)
    {
        _orders = orders;
        _carts = carts;
        _events = events;
        _catalog = catalog;
        _httpContextAccessor = httpContextAccessor;
        _logger = logger;
    }

    private string GetCorrelationId() =>
        _httpContextAccessor.HttpContext?.Items[CorrelationIdMiddleware.ItemsKey] as string
        ?? Guid.NewGuid().ToString("N");

    public async Task<OrderResponse> CheckoutAsync(Guid userId, CheckoutRequest request)
    {
        if (string.IsNullOrWhiteSpace(request.ShippingAddress))
            throw new ArgumentException("Shipping address is required.");

        if (string.IsNullOrWhiteSpace(request.ShippingPincode) || request.ShippingPincode.Trim().Length != 6)
            throw new ArgumentException("Valid 6-digit pincode is required.");

        var cart = await _carts.GetByUserIdAsync(userId);

        if (cart is null || cart.Items.Count == 0)
            throw new InvalidOperationException("Cart is empty.");

        // Validate current stock for all items before creating the order
        var stockResults = await Task.WhenAll(cart.Items.Select(async item =>
        {
            var stock = await _catalog.GetStockAsync(item.ProductId);
            return (item, stock);
        }));

        var insufficient = stockResults.Where(x => x.stock < x.item.Quantity).ToList();
        if (insufficient.Count > 0)
        {
            var names = string.Join(", ", insufficient.Select(x => x.item.ProductName));
            throw new InvalidOperationException($"Insufficient stock for: {names}");
        }

        var order = new Order
        {
            Id = Guid.NewGuid(),
            UserId = userId,
            Status = OrderStatus.PaymentPending,
            ShippingAddress = request.ShippingAddress.Trim(),
            ShippingCity = request.ShippingCity.Trim(),
            ShippingPincode = request.ShippingPincode.Trim(),
            TotalAmount = cart.Items.Sum(i => i.Price * i.Quantity)
        };

        foreach (var cartItem in cart.Items)
        {
            order.Items.Add(new OrderItem
            {
                Id = Guid.NewGuid(),
                OrderId = order.Id,
                ProductId = cartItem.ProductId,
                ProductName = cartItem.ProductName,
                Price = cartItem.Price,
                ImageUrl = cartItem.ImageUrl,
                Quantity = cartItem.Quantity
            });
        }

        await _orders.AddAsync(order);
        await _carts.ClearCartAsync(cart);

        _logger.LogInformation("Order {OrderId} created for user {UserId}, amount: {Amount}", order.Id, userId, order.TotalAmount);

        return MapToOrderResponse(order);
    }

    public async Task UpdatePaymentStatusAsync(Guid orderId, Guid userId, string status, string paymentMethod, DateTime? paidAt)
    {
        var order = await _orders.GetByIdAsync(orderId);

        if (order is null || order.UserId != userId)
            throw new KeyNotFoundException("Order not found.");

        if (order.Status != OrderStatus.PaymentPending)
            throw new InvalidOperationException("Order is not awaiting payment.");

        if (Enum.TryParse<PaymentMethod>(paymentMethod, ignoreCase: true, out var method))
            order.PaymentMethod = method;

        order.Status = status == "Paid" ? OrderStatus.Paid : OrderStatus.PaymentFailed;
        order.PaidAt = paidAt;

        await _orders.UpdateAsync(order);

        _logger.LogInformation(
            "Order {OrderId} payment status updated to {Status} by PaymentService", orderId, status);

        if (order.Status == OrderStatus.Paid)
        {
            var items = order.Items
                .Select(i => new StockAdjustItem(i.ProductId, -i.Quantity))
                .ToList();

            await _events.PublishAsync(QueueNames.StockAdjustmentRequested,
                new StockAdjustmentRequestedEvent(orderId, items, GetCorrelationId()));
        }
    }

    public async Task<List<OrderListResponse>> GetMyOrdersAsync(Guid userId)
    {
        var orders = await _orders.GetByUserIdAsync(userId);

        return orders.Select(o => new OrderListResponse
        {
            Id = o.Id,
            Status = o.Status.ToString(),
            TotalAmount = o.TotalAmount,
            ItemCount = o.Items.Count,
            CreatedAt = o.CreatedAt
        }).ToList();
    }

    public async Task<OrderResponse> GetOrderByIdAsync(Guid userId, Guid orderId)
    {
        var order = await _orders.GetByIdAsync(orderId);

        if (order is null || order.UserId != userId)
            throw new KeyNotFoundException("Order not found.");

        return MapToOrderResponse(order);
    }

    public async Task CancelOrderAsync(Guid userId, Guid orderId)
    {
        var order = await _orders.GetByIdAsync(orderId);

        if (order is null || order.UserId != userId)
            throw new KeyNotFoundException("Order not found.");

        if (order.Status >= OrderStatus.Packed)
            throw new InvalidOperationException("Cannot cancel order after it has been packed.");

        var wasStockDecremented = order.Status == OrderStatus.Paid;

        order.Status = OrderStatus.Cancelled;
        await _orders.UpdateAsync(order);

        // CatalogService restores stock via OrderCancelledEvent — no synchronous HTTP call needed.
        var stockItems = order.Items
            .Select(i => new StockAdjustItem(i.ProductId, i.Quantity))
            .ToList();

        await _events.PublishAsync(QueueNames.OrderCancelled, new OrderCancelledEvent(
            OrderId: orderId,
            UserId: userId,
            TotalAmount: order.TotalAmount,
            CancelledAt: DateTime.UtcNow,
            WasPaid: wasStockDecremented,
            Items: stockItems,
            CorrelationId: GetCorrelationId()));

        _logger.LogInformation("Order {OrderId} cancelled by user {UserId}", orderId, userId);
    }

    private static OrderResponse MapToOrderResponse(Order order)
    {
        return new OrderResponse
        {
            Id = order.Id,
            Status = order.Status.ToString(),
            TotalAmount = order.TotalAmount,
            PaymentMethod = order.PaymentMethod?.ToString(),
            ShippingAddress = order.ShippingAddress,
            ShippingCity = order.ShippingCity,
            ShippingPincode = order.ShippingPincode,
            CreatedAt = order.CreatedAt,
            PaidAt = order.PaidAt,
            Items = order.Items.Select(i => new OrderItemResponse
            {
                ProductId = i.ProductId,
                ProductName = i.ProductName,
                Price = i.Price,
                ImageUrl = i.ImageUrl,
                Quantity = i.Quantity,
                Subtotal = i.Price * i.Quantity
            }).ToList()
        };
    }
}
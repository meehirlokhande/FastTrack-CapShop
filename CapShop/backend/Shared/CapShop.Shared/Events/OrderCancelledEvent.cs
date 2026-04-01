namespace CapShop.Shared.Events;

public record OrderCancelledEvent(
    Guid OrderId,
    Guid UserId,
    decimal TotalAmount,
    DateTime CancelledAt);

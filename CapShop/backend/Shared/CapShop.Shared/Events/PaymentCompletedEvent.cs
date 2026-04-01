namespace CapShop.Shared.Events;

public record PaymentCompletedEvent(
    Guid TransactionId,
    Guid OrderId,
    Guid UserId,
    decimal Amount,
    string Status,
    string PaymentMethod,
    DateTime CompletedAt);

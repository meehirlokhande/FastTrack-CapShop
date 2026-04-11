namespace CapShop.Shared.Events;

public record StockAdjustmentRequestedEvent(
    Guid OrderId,
    IReadOnlyList<StockAdjustItem> Items,
    string CorrelationId) : ICorrelatedEvent;

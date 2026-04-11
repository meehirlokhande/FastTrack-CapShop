namespace CapShop.Shared.Events;

// These constants are fanout exchange names.
// Publishers publish to the exchange; each consumer binds its own durable queue.
public static class QueueNames
{
    public const string PaymentCompleted = "capshop.payment.completed";
    public const string OrderCancelled = "capshop.order.cancelled";
    public const string StockAdjustmentRequested = "capshop.stock.adjustment.requested";
}

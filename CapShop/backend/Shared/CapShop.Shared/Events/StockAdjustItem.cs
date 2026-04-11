namespace CapShop.Shared.Events;

public record StockAdjustItem(Guid ProductId, int Delta);

namespace CapShop.OrderService.Services;

public interface ICatalogHttpClient
{
    Task<int> GetStockAsync(Guid productId);
}

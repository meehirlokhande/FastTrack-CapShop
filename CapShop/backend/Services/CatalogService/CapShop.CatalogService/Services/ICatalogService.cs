using CapShop.CatalogService.Dtos;

namespace CapShop.CatalogService.Services;

public interface ICatalogService
{
    Task<ProductResponse> GetProductByIdAsync(Guid id);
    Task<PagedResponse<ProductResponse>> GetProductsAsync(ProductListRequest request);
    Task<List<ProductResponse>> GetFeaturedProductsAsync();
    Task<List<CategoryResponse>> GetCategoriesAsync();
    Task AdjustStockBatchAsync(IEnumerable<StockAdjustItem> items);
}
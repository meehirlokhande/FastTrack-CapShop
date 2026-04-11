using CapShop.CatalogService.Dtos;
using CapShop.CatalogService.Models;

namespace CapShop.CatalogService.Repositories
{
    public interface IProductRepository
    {
        Task<Product?> GetByIdAsync(Guid id);
        Task<(List<Product> Items, int TotalCount)> GetFilteredAsync(ProductListRequest request);
        Task<List<Product>> GetFeaturedAsync(int count);
        Task<List<Category>> GetAllCategoriesAsync();
        Task AdjustStockBatchAsync(IEnumerable<(Guid ProductId, int Delta)> adjustments);
    }
}

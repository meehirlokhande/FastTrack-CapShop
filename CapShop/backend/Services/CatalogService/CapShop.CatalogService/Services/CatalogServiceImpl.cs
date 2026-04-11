using System.Text.Json;
using CapShop.CatalogService.Dtos;
using CapShop.CatalogService.Models;
using CapShop.CatalogService.Repositories;
using Microsoft.Extensions.Caching.Distributed;

namespace CapShop.CatalogService.Services
{
    public class CatalogServiceImpl : ICatalogService
    {
        private readonly IProductRepository _products;
        private readonly IDistributedCache _cache;
        private readonly ILogger<CatalogServiceImpl> _logger;

        // Cache keys — must match what AdminService invalidates
        public const string KeyCategories = "categories";
        public const string KeyFeatured = "featured";
        public static string KeyProduct(Guid id) => $"product:{id}";

        private static readonly DistributedCacheEntryOptions CategoriesTtl =
            new() { AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(10) };

        private static readonly DistributedCacheEntryOptions FeaturedTtl =
            new() { AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(5) };

        private static readonly DistributedCacheEntryOptions ProductTtl =
            new() { AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(5) };

        public CatalogServiceImpl(
            IProductRepository products,
            IDistributedCache cache,
            ILogger<CatalogServiceImpl> logger)
        {
            _products = products;
            _cache = cache;
            _logger = logger;
        }

        public async Task<ProductResponse> GetProductByIdAsync(Guid id)
        {
            var key = KeyProduct(id);
            var cached = await _cache.GetStringAsync(key);
            if (cached is not null)
            {
                _logger.LogInformation("Cache hit: {Key}", key);
                return JsonSerializer.Deserialize<ProductResponse>(cached)!;
            }

            var product = await _products.GetByIdAsync(id);
            if (product is null)
                throw new KeyNotFoundException("Product not found.");

            var response = MapProduct(product);
            await _cache.SetStringAsync(key, JsonSerializer.Serialize(response), ProductTtl);

            return response;
        }

        public async Task<PagedResponse<ProductResponse>> GetProductsAsync(ProductListRequest request)
        {
            var (items, totalCount) = await _products.GetFilteredAsync(request);
            var productDtos = items.Select(MapProduct).ToList();

            _logger.LogInformation("Products query: {Query}, Page: {Page}, Results: {Count}",
                request.Query, request.Page, totalCount);

            return new PagedResponse<ProductResponse>
            {
                Items = productDtos,
                TotalCount = totalCount,
                Page = request.Page,
                PageSize = request.PageSize,
                TotalPages = (int)Math.Ceiling(totalCount / (double)request.PageSize)
            };
        }

        public async Task<List<ProductResponse>> GetFeaturedProductsAsync()
        {
            var cached = await _cache.GetStringAsync(KeyFeatured);
            if (cached is not null)
            {
                _logger.LogInformation("Cache hit: {Key}", KeyFeatured);
                return JsonSerializer.Deserialize<List<ProductResponse>>(cached)!;
            }

            var products = await _products.GetFeaturedAsync(8);
            var response = products.Select(MapProduct).ToList();

            await _cache.SetStringAsync(KeyFeatured, JsonSerializer.Serialize(response), FeaturedTtl);

            return response;
        }

        public async Task<List<CategoryResponse>> GetCategoriesAsync()
        {
            var cached = await _cache.GetStringAsync(KeyCategories);
            if (cached is not null)
            {
                _logger.LogInformation("Cache hit: {Key}", KeyCategories);
                return JsonSerializer.Deserialize<List<CategoryResponse>>(cached)!;
            }

            var categories = await _products.GetAllCategoriesAsync();
            var response = categories.Select(c => new CategoryResponse
            {
                Id = c.Id,
                Name = c.Name,
                Description = c.Description
            }).ToList();

            await _cache.SetStringAsync(KeyCategories, JsonSerializer.Serialize(response), CategoriesTtl);

            return response;
        }

        public async Task AdjustStockBatchAsync(IEnumerable<StockAdjustItem> items)
        {
            var itemsList = items.ToList();

            await _products.AdjustStockBatchAsync(itemsList.Select(i => (i.ProductId, i.Delta)));

            foreach (var item in itemsList)
                await _cache.RemoveAsync(KeyProduct(item.ProductId));

            await _cache.RemoveAsync(KeyFeatured);

            _logger.LogInformation("Stock adjusted for {Count} product(s)", itemsList.Count);
        }

        private static ProductResponse MapProduct(Product p) => new()
        {
            Id = p.Id,
            Name = p.Name,
            Description = p.Description,
            Price = p.Price,
            DiscountPrice = p.DiscountPrice,
            Stock = p.Stock,
            ImageUrl = p.ImageUrl,
            IsFeatured = p.IsFeatured,
            Status = p.Status.ToString(),
            CategoryName = p.Category.Name
        };
    }
}

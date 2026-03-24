using CapShop.CatalogService.Dtos;
using CapShop.CatalogService.Models;
using CapShop.CatalogService.Repositories;

namespace CapShop.CatalogService.Services
{
    public class CatalogServiceImpl:ICatalogService
    {
        private readonly IProductRepository _products;
        private readonly ILogger<CatalogServiceImpl> _logger;

        public CatalogServiceImpl(IProductRepository products, ILogger<CatalogServiceImpl> logger)
        {
            _products = products;
            _logger = logger;
        }

        public async Task<ProductResponse> GetProductByIdAsync(Guid id)
        {
            var product = await _products.GetByIdAsync(id);
            if (product is null)
                throw new KeyNotFoundException("Product not found.");
            return new ProductResponse
            {
                Id = product.Id,
                Name = product.Name,
                Description = product.Description,
                Price = product.Price,
                DiscountPrice = product.DiscountPrice,
                Stock = product.Stock,
                ImageUrl = product.ImageUrl,
                IsFeatured = product.IsFeatured,
                Status = product.Status.ToString(),
                CategoryName = product.Category.Name
            };
        }

        public async Task<PagedResponse<ProductResponse>> GetProductsAsync(ProductListRequest request)
        {
            var (items, totalCount) = await _products.GetFilteredAsync(request);
            var productDtos = items.Select(p => new ProductResponse
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
            }).ToList();
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
            var products = await _products.GetFeaturedAsync(8);
            return products.Select(p => new ProductResponse
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
            }).ToList();
        }

        public async Task<List<CategoryResponse>> GetCategoriesAsync()
        {
            var categories = await _products.GetAllCategoriesAsync();
            return categories.Select(c => new CategoryResponse
            {
                Id = c.Id,
                Name = c.Name,
                Description = c.Description
            }).ToList();
        }
    }
}

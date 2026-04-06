using CapShop.AdminService.Dtos;
using CapShop.AdminService.Models;
using CapShop.AdminService.Repositories;
using Microsoft.Extensions.Caching.Distributed;

namespace CapShop.AdminService.Services;

public class ProductManagementService : IProductManagementService
{
    private readonly IAdminProductRepository _products;
    private readonly IAdminLogService _log;
    private readonly IDistributedCache _cache;
    private readonly ILogger<ProductManagementService> _logger;

    // Must match the instance name prefix + keys used in CatalogService
    private const string KeyFeatured = "featured";
    private static string KeyProduct(Guid id) => $"product:{id}";

    public ProductManagementService(
        IAdminProductRepository products,
        IAdminLogService log,
        IDistributedCache cache,
        ILogger<ProductManagementService> logger)
    {
        _products = products;
        _log = log;
        _cache = cache;
        _logger = logger;
    }

    public async Task<List<AdminProductResponse>> GetAllProductsAsync()
    {
        var products = await _products.GetAllAsync();
        return products.Select(MapToResponse).ToList();
    }

    public async Task<AdminProductResponse> GetProductByIdAsync(Guid id)
    {
        var product = await _products.GetByIdAsync(id)
            ?? throw new KeyNotFoundException("Product not found.");
        return MapToResponse(product);
    }

    public async Task<AdminProductResponse> CreateProductAsync(Guid adminUserId, CreateProductRequest request)
    {
        if (request.Price <= 0)
            throw new ArgumentException("Price must be greater than zero.");

        if (request.Stock < 0)
            throw new ArgumentException("Stock cannot be negative.");

        if (!await _products.CategoryExistsAsync(request.CategoryId))
            throw new KeyNotFoundException("Category not found.");

        var product = new Product
        {
            Id = Guid.NewGuid(),
            Name = request.Name.Trim(),
            Description = request.Description.Trim(),
            Price = request.Price,
            DiscountPrice = request.DiscountPrice,
            Stock = request.Stock,
            ImageUrl = request.ImageUrl.Trim(),
            IsFeatured = request.IsFeatured,
            CategoryId = request.CategoryId
        };

        await _products.AddAsync(product);

        if (product.IsFeatured)
            await _cache.RemoveAsync(KeyFeatured);

        await _log.LogAsync(adminUserId, "Created", "Product", product.Id.ToString(),
            $"Created product: {product.Name}, Price: {product.Price}");

        _logger.LogInformation("Product created: {ProductId} - {Name}", product.Id, product.Name);

        var saved = await _products.GetByIdAsync(product.Id);
        return MapToResponse(saved!);
    }

    public async Task<AdminProductResponse> UpdateProductAsync(Guid adminUserId, Guid id, UpdateProductRequest request)
    {
        var product = await _products.GetByIdAsync(id)
            ?? throw new KeyNotFoundException("Product not found.");

        if (request.Price <= 0)
            throw new ArgumentException("Price must be greater than zero.");

        if (!await _products.CategoryExistsAsync(request.CategoryId))
            throw new KeyNotFoundException("Category not found.");

        product.Name = request.Name.Trim();
        product.Description = request.Description.Trim();
        product.Price = request.Price;
        product.DiscountPrice = request.DiscountPrice;
        product.Stock = request.Stock;
        product.ImageUrl = request.ImageUrl.Trim();
        product.IsFeatured = request.IsFeatured;
        product.CategoryId = request.CategoryId;

        await _products.UpdateAsync(product);

        await _cache.RemoveAsync(KeyProduct(id));
        await _cache.RemoveAsync(KeyFeatured);

        await _log.LogAsync(adminUserId, "Updated", "Product", id.ToString(),
            $"Updated product: {product.Name}");

        var updated = await _products.GetByIdAsync(id);
        return MapToResponse(updated!);
    }

    public async Task DeleteProductAsync(Guid adminUserId, Guid id)
    {
        var product = await _products.GetByIdAsync(id)
            ?? throw new KeyNotFoundException("Product not found.");

        await _products.DeleteAsync(product);

        await _cache.RemoveAsync(KeyProduct(id));
        await _cache.RemoveAsync(KeyFeatured);

        await _log.LogAsync(adminUserId, "Deleted", "Product", id.ToString(),
            $"Deleted product: {product.Name}");

        _logger.LogInformation("Product deleted: {ProductId}", id);
    }

    public async Task<AdminProductResponse> ToggleStatusAsync(Guid adminUserId, Guid id)
    {
        var product = await _products.GetByIdAsync(id)
            ?? throw new KeyNotFoundException("Product not found.");

        var oldStatus = product.Status;
        product.Status = product.Status == ProductStatus.Active
            ? ProductStatus.Inactive
            : ProductStatus.Active;

        await _products.UpdateAsync(product);

        await _cache.RemoveAsync(KeyProduct(id));
        await _cache.RemoveAsync(KeyFeatured);

        await _log.LogAsync(adminUserId, "StatusChanged", "Product", id.ToString(),
            $"Status changed from {oldStatus} to {product.Status}");

        return MapToResponse(product);
    }

    public async Task<AdminProductResponse> UpdateStockAsync(Guid adminUserId, Guid id, UpdateStockRequest request)
    {
        if (request.Stock < 0)
            throw new ArgumentException("Stock cannot be negative.");

        var product = await _products.GetByIdAsync(id)
            ?? throw new KeyNotFoundException("Product not found.");

        var oldStock = product.Stock;
        product.Stock = request.Stock;
        await _products.UpdateAsync(product);

        await _cache.RemoveAsync(KeyProduct(id));

        await _log.LogAsync(adminUserId, "StockUpdated", "Product", id.ToString(),
            $"Stock changed from {oldStock} to {request.Stock}");

        return MapToResponse(product);
    }

    private static AdminProductResponse MapToResponse(Product p)
    {
        return new AdminProductResponse
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
            CategoryName = p.Category?.Name ?? string.Empty,
            CreatedAt = p.CreatedAt
        };
    }
}
using CapShop.CatalogService.Data;
using CapShop.CatalogService.Dtos;
using CapShop.CatalogService.Models;
using Microsoft.EntityFrameworkCore;

namespace CapShop.CatalogService.Repositories;

public class ProductRepository : IProductRepository
{
    private readonly CatalogDbContext _db;

    public ProductRepository(CatalogDbContext db)
    {
        _db = db;
    }

    public async Task<Product?> GetByIdAsync(Guid id)
    {
        return await _db.Products
            .Include(p => p.Category)
            .FirstOrDefaultAsync(p => p.Id == id);
    }

    public async Task<(List<Product> Items, int TotalCount)> GetFilteredAsync(ProductListRequest request)
    {
        var query = _db.Products
            .Include(p => p.Category)
            .Where(p => p.Status == ProductStatus.Active)
            .AsQueryable();

        if (!string.IsNullOrWhiteSpace(request.Query))
        {
            var search = request.Query.Trim().ToLower();
            query = query.Where(p =>
                p.Name.ToLower().Contains(search) ||
                p.Description.ToLower().Contains(search));
        }

        if (!string.IsNullOrWhiteSpace(request.Category))
        {
            query = query.Where(p => p.Category.Name == request.Category);
        }

        if (request.MinPrice.HasValue)
        {
            query = query.Where(p => p.Price >= request.MinPrice.Value);
        }

        if (request.MaxPrice.HasValue)
        {
            query = query.Where(p => p.Price <= request.MaxPrice.Value);
        }

        query = request.Sort?.ToLower() switch
        {
            "price_asc" => query.OrderBy(p => p.Price),
            "price_desc" => query.OrderByDescending(p => p.Price),
            "name" => query.OrderBy(p => p.Name),
            "newest" => query.OrderByDescending(p => p.CreatedAt),
            _ => query.OrderByDescending(p => p.CreatedAt)
        };

        var totalCount = await query.CountAsync();

        var items = await query
            .Skip((request.Page - 1) * request.PageSize)
            .Take(request.PageSize)
            .ToListAsync();

        return (items, totalCount);
    }

    public async Task<List<Product>> GetFeaturedAsync(int count)
    {
        return await _db.Products
            .Include(p => p.Category)
            .Where(p => p.IsFeatured && p.Status == ProductStatus.Active)
            .OrderByDescending(p => p.CreatedAt)
            .Take(count)
            .ToListAsync();
    }

    public async Task<List<Category>> GetAllCategoriesAsync()
    {
        return await _db.Categories
            .OrderBy(c => c.Name)
            .ToListAsync();
    }
}
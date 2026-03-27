using CapShop.AdminService.Data;
using CapShop.AdminService.Models;
using Microsoft.EntityFrameworkCore;

namespace CapShop.AdminService.Repositories;

public class AdminProductRepository : IAdminProductRepository
{
    private readonly AdminCatalogDbContext _db;

    public AdminProductRepository(AdminCatalogDbContext db)
    {
        _db = db;
    }

    public async Task<List<Product>> GetAllAsync()
    {
        return await _db.Products
            .Include(p => p.Category)
            .OrderByDescending(p => p.CreatedAt)
            .ToListAsync();
    }

    public async Task<Product?> GetByIdAsync(Guid id)
    {
        return await _db.Products
            .Include(p => p.Category)
            .FirstOrDefaultAsync(p => p.Id == id);
    }

    public async Task AddAsync(Product product)
    {
        _db.Products.Add(product);
        await _db.SaveChangesAsync();
    }

    public async Task UpdateAsync(Product product)
    {
        _db.Products.Update(product);
        await _db.SaveChangesAsync();
    }

    public async Task DeleteAsync(Product product)
    {
        _db.Products.Remove(product);
        await _db.SaveChangesAsync();
    }

    public async Task<int> GetTotalCountAsync()
    {
        return await _db.Products.CountAsync();
    }

    public async Task<bool> CategoryExistsAsync(int categoryId)
    {
        return await _db.Categories.AnyAsync(c => c.Id == categoryId);
    }
}
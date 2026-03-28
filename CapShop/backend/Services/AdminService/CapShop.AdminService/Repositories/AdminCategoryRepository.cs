using CapShop.AdminService.Data;
using CapShop.AdminService.Models;
using Microsoft.EntityFrameworkCore;

namespace CapShop.AdminService.Repositories;

public class AdminCategoryRepository : IAdminCategoryRepository
{
    private readonly AdminCatalogDbContext _db;

    public AdminCategoryRepository(AdminCatalogDbContext db)
    {
        _db = db;
    }

    public async Task<List<Category>> GetAllAsync()
    {
        return await _db.Categories
            .Include(c => c.Products)
            .OrderBy(c => c.Name)
            .ToListAsync();
    }

    public async Task<Category?> GetByIdAsync(int id)
    {
        return await _db.Categories
            .Include(c => c.Products)
            .FirstOrDefaultAsync(c => c.Id == id);
    }

    public async Task<bool> ExistsByNameAsync(string name)
    {
        return await _db.Categories
            .AnyAsync(c => c.Name.ToLower() == name.ToLower());
    }

    public async Task<bool> HasProductsAsync(int id)
    {
        return await _db.Products.AnyAsync(p => p.CategoryId == id);
    }

    public async Task AddAsync(Category category)
    {
        _db.Categories.Add(category);
        await _db.SaveChangesAsync();
    }

    public async Task DeleteAsync(Category category)
    {
        _db.Categories.Remove(category);
        await _db.SaveChangesAsync();
    }
}

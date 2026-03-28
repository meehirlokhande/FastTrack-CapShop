using CapShop.AdminService.Models;

namespace CapShop.AdminService.Repositories;

public interface IAdminCategoryRepository
{
    Task<List<Category>> GetAllAsync();
    Task<Category?> GetByIdAsync(int id);
    Task<bool> ExistsByNameAsync(string name);
    Task<bool> HasProductsAsync(int id);
    Task AddAsync(Category category);
    Task DeleteAsync(Category category);
}

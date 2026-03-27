using CapShop.AdminService.Models;

namespace CapShop.AdminService.Repositories;

public interface IAdminProductRepository
{
    Task<List<Product>> GetAllAsync();
    Task<Product?> GetByIdAsync(Guid id);
    Task AddAsync(Product product);
    Task UpdateAsync(Product product);
    Task DeleteAsync(Product product);
    Task<int> GetTotalCountAsync();
    Task<bool> CategoryExistsAsync(int categoryId);
}
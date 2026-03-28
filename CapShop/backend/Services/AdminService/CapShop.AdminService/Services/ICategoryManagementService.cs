using CapShop.AdminService.Dtos;

namespace CapShop.AdminService.Services;

public interface ICategoryManagementService
{
    Task<List<AdminCategoryResponse>> GetAllAsync();
    Task<AdminCategoryResponse> CreateAsync(CreateCategoryRequest request);
    Task DeleteAsync(int id);
}

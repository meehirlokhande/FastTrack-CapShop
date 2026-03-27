using CapShop.AdminService.Dtos;

namespace CapShop.AdminService.Services;

public interface IProductManagementService
{
    Task<List<AdminProductResponse>> GetAllProductsAsync();
    Task<AdminProductResponse> GetProductByIdAsync(Guid id);
    Task<AdminProductResponse> CreateProductAsync(Guid adminUserId, CreateProductRequest request);
    Task<AdminProductResponse> UpdateProductAsync(Guid adminUserId, Guid id, UpdateProductRequest request);
    Task DeleteProductAsync(Guid adminUserId, Guid id);
    Task<AdminProductResponse> ToggleStatusAsync(Guid adminUserId, Guid id);
    Task<AdminProductResponse> UpdateStockAsync(Guid adminUserId, Guid id, UpdateStockRequest request);
}
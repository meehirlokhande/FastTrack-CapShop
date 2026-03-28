using CapShop.AdminService.Dtos;
using CapShop.AdminService.Models;
using CapShop.AdminService.Repositories;

namespace CapShop.AdminService.Services;

public class CategoryManagementService : ICategoryManagementService
{
    private readonly IAdminCategoryRepository _categories;

    public CategoryManagementService(IAdminCategoryRepository categories)
    {
        _categories = categories;
    }

    public async Task<List<AdminCategoryResponse>> GetAllAsync()
    {
        var categories = await _categories.GetAllAsync();
        return categories.Select(c => new AdminCategoryResponse
        {
            Id = c.Id,
            Name = c.Name,
            Description = c.Description,
            ProductCount = c.Products.Count
        }).ToList();
    }

    public async Task<AdminCategoryResponse> CreateAsync(CreateCategoryRequest request)
    {
        if (string.IsNullOrWhiteSpace(request.Name))
            throw new ArgumentException("Category name is required.");

        if (await _categories.ExistsByNameAsync(request.Name.Trim()))
            throw new InvalidOperationException($"Category '{request.Name.Trim()}' already exists.");

        var category = new Category
        {
            Name = request.Name.Trim(),
            Description = request.Description.Trim()
        };

        await _categories.AddAsync(category);

        return new AdminCategoryResponse
        {
            Id = category.Id,
            Name = category.Name,
            Description = category.Description,
            ProductCount = 0
        };
    }

    public async Task DeleteAsync(int id)
    {
        var category = await _categories.GetByIdAsync(id)
            ?? throw new KeyNotFoundException("Category not found.");

        if (await _categories.HasProductsAsync(id))
            throw new InvalidOperationException(
                "Cannot delete a category that has products assigned to it. Reassign or delete those products first.");

        await _categories.DeleteAsync(category);
    }
}

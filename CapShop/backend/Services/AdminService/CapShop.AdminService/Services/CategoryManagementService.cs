using CapShop.AdminService.Dtos;
using CapShop.AdminService.Models;
using CapShop.AdminService.Repositories;
using Microsoft.Extensions.Caching.Distributed;

namespace CapShop.AdminService.Services;

public class CategoryManagementService : ICategoryManagementService
{
    private readonly IAdminCategoryRepository _categories;
    private readonly IDistributedCache _cache;
    private const string KeyCategories = "categories";

    public CategoryManagementService(IAdminCategoryRepository categories, IDistributedCache cache)
    {
        _categories = categories;
        _cache = cache;
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
        await _cache.RemoveAsync(KeyCategories);

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
        await _cache.RemoveAsync(KeyCategories);
    }
}

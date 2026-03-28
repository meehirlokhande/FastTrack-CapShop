using CapShop.AdminService.Dtos;
using CapShop.AdminService.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace CapShop.AdminService.Controllers;

[ApiController]
[Route("admin/categories")]
[Authorize(Roles = "Admin")]
public class AdminCategoryController : ControllerBase
{
    private readonly ICategoryManagementService _categories;

    public AdminCategoryController(ICategoryManagementService categories)
    {
        _categories = categories;
    }

    [HttpGet]
    public async Task<IActionResult> GetAll()
    {
        var result = await _categories.GetAllAsync();
        return Ok(result);
    }

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreateCategoryRequest request)
    {
        var result = await _categories.CreateAsync(request);
        return Ok(result);
    }

    [HttpDelete("{id:int}")]
    public async Task<IActionResult> Delete(int id)
    {
        await _categories.DeleteAsync(id);
        return Ok(new { message = "Category deleted." });
    }
}

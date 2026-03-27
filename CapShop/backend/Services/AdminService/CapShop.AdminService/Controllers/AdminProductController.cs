using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using CapShop.AdminService.Dtos;
using CapShop.AdminService.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace CapShop.AdminService.Controllers;

[ApiController]
[Route("admin/products")]
[Authorize(Roles = "Admin")]
public class AdminProductController : ControllerBase
{
    private readonly IProductManagementService _products;

    public AdminProductController(IProductManagementService products)
    {
        _products = products;
    }

    private Guid GetAdminUserId()
    {
        var sub = User.FindFirstValue(ClaimTypes.NameIdentifier)
                  ?? User.FindFirstValue(JwtRegisteredClaimNames.Sub);
        return Guid.Parse(sub!);
    }

    [HttpGet]
    public async Task<IActionResult> GetAll()
    {
        var result = await _products.GetAllProductsAsync();
        return Ok(result);
    }

    [HttpGet("{id:guid}")]
    public async Task<IActionResult> GetById(Guid id)
    {
        var result = await _products.GetProductByIdAsync(id);
        return Ok(result);
    }

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreateProductRequest request)
    {
        var result = await _products.CreateProductAsync(GetAdminUserId(), request);
        return Ok(result);
    }

    [HttpPut("{id:guid}")]
    public async Task<IActionResult> Update(Guid id, [FromBody] UpdateProductRequest request)
    {
        var result = await _products.UpdateProductAsync(GetAdminUserId(), id, request);
        return Ok(result);
    }

    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> Delete(Guid id)
    {
        await _products.DeleteProductAsync(GetAdminUserId(), id);
        return Ok(new { message = "Product deleted." });
    }

    [HttpPut("{id:guid}/toggle-status")]
    public async Task<IActionResult> ToggleStatus(Guid id)
    {
        var result = await _products.ToggleStatusAsync(GetAdminUserId(), id);
        return Ok(result);
    }

    [HttpPut("{id:guid}/stock")]
    public async Task<IActionResult> UpdateStock(Guid id, [FromBody] UpdateStockRequest request)
    {
        var result = await _products.UpdateStockAsync(GetAdminUserId(), id, request);
        return Ok(result);
    }
}
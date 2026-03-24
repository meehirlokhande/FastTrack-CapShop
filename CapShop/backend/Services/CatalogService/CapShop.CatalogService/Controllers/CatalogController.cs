using CapShop.CatalogService.Dtos;
using CapShop.CatalogService.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace CapShop.CatalogService.Controllers;

[ApiController]
[Route("catalog")]
public class CatalogController : ControllerBase
{
    private readonly ICatalogService _catalog;

    public CatalogController(ICatalogService catalog)
    {
        _catalog = catalog;
    }

    [HttpGet("health")]
    [AllowAnonymous]
    public IActionResult Health()
    {
        return Ok(new { service = "CatalogService", status = "Healthy" });
    }

    [HttpGet("categories")]
    [AllowAnonymous]
    public async Task<IActionResult> GetCategories()
    {
        var categories = await _catalog.GetCategoriesAsync();
        return Ok(categories);
    }

    [HttpGet("products")]
    [AllowAnonymous]
    public async Task<IActionResult> GetProducts([FromQuery] ProductListRequest request)
    {
        var result = await _catalog.GetProductsAsync(request);
        return Ok(result);
    }

    [HttpGet("products/featured")]
    [AllowAnonymous]
    public async Task<IActionResult> GetFeaturedProducts()
    {
        var products = await _catalog.GetFeaturedProductsAsync();
        return Ok(products);
    }

    [HttpGet("products/{id:guid}")]
    [AllowAnonymous]
    public async Task<IActionResult> GetProductById(Guid id)
    {
        var product = await _catalog.GetProductByIdAsync(id);
        return Ok(product);
    }
}
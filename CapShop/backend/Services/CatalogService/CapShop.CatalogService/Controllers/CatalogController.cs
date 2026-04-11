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
    private readonly IConfiguration _config;

    public CatalogController(ICatalogService catalog, IConfiguration config)
    {
        _catalog = catalog;
        _config = config;
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

    [HttpPatch("products/stock/batch-adjust")]
    public async Task<IActionResult> BatchAdjustStock([FromBody] List<StockAdjustItem> items)
    {
        var internalKey = Request.Headers["X-Internal-Key"].FirstOrDefault();
        var expectedKey = _config["InternalApi:Key"];

        if (string.IsNullOrEmpty(internalKey) || internalKey != expectedKey)
            return Unauthorized(new { message = "Invalid or missing internal API key." });

        if (items is null || items.Count == 0)
            return BadRequest(new { message = "No items provided." });

        await _catalog.AdjustStockBatchAsync(items);
        return NoContent();
    }
}
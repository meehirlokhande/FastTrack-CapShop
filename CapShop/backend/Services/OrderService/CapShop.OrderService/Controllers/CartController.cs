using System.Security.Claims;
using CapShop.OrderService.Dtos;
using CapShop.OrderService.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace CapShop.OrderService.Controllers;

[ApiController]
[Route("orders/cart")]
[Authorize]
public class CartController : ControllerBase
{
    private readonly ICartService _cart;

    public CartController(ICartService cart)
    {
        _cart = cart;
    }

    private Guid GetUserId()
    {
        var sub = User.FindFirstValue(ClaimTypes.NameIdentifier)
                  ?? User.FindFirstValue("sub");
        return Guid.Parse(sub!);
    }

    [HttpGet]
    public async Task<IActionResult> GetCart()
    {
        var result = await _cart.GetCartAsync(GetUserId());
        return Ok(result);
    }

    [HttpPost("items")]
    public async Task<IActionResult> AddItem([FromBody] AddCartItemRequest request)
    {
        var result = await _cart.AddItemAsync(GetUserId(), request);
        return Ok(result);
    }

    [HttpPut("items/{id:guid}")]
    public async Task<IActionResult> UpdateItem(Guid id, [FromBody] UpdateCartItemRequest request)
    {
        var result = await _cart.UpdateItemAsync(GetUserId(), id, request);
        return Ok(result);
    }

    [HttpDelete("items/{id:guid}")]
    public async Task<IActionResult> RemoveItem(Guid id)
    {
        await _cart.RemoveItemAsync(GetUserId(), id);
        return NoContent();
    }
}
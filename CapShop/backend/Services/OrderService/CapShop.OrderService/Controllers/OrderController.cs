using System.Security.Claims;
using CapShop.OrderService.Dtos;
using CapShop.OrderService.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace CapShop.OrderService.Controllers;

[ApiController]
[Route("orders")]
[Authorize]
public class OrderController : ControllerBase
{
    private readonly IOrderManagementService _orders;

    public OrderController(IOrderManagementService orders)
    {
        _orders = orders;
    }

    private Guid GetUserId()
    {
        var sub = User.FindFirstValue(ClaimTypes.NameIdentifier)
                  ?? User.FindFirstValue("sub");
        return Guid.Parse(sub!);
    }

    [HttpGet("health")]
    [AllowAnonymous]
    public IActionResult Health()
    {
        return Ok(new { service = "OrderService", status = "Healthy" });
    }

    [HttpPost("checkout")]
    public async Task<IActionResult> Checkout([FromBody] CheckoutRequest request)
    {
        var result = await _orders.CheckoutAsync(GetUserId(), request);
        return Ok(result);
    }

    [HttpPost("payment/simulate")]
    public async Task<IActionResult> SimulatePayment([FromBody] PaymentRequest request)
    {
        var result = await _orders.SimulatePaymentAsync(GetUserId(), request);
        return Ok(result);
    }

    [HttpGet("my")]
    public async Task<IActionResult> GetMyOrders()
    {
        var result = await _orders.GetMyOrdersAsync(GetUserId());
        return Ok(result);
    }

    [HttpGet("{id:guid}")]
    public async Task<IActionResult> GetOrderById(Guid id)
    {
        var result = await _orders.GetOrderByIdAsync(GetUserId(), id);
        return Ok(result);
    }

    [HttpPut("{id:guid}/cancel")]
    public async Task<IActionResult> CancelOrder(Guid id)
    {
        await _orders.CancelOrderAsync(GetUserId(), id);
        return Ok(new { message = "Order cancelled." });
    }
}
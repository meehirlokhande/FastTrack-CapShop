using System.Security.Claims;
using CapShop.PaymentService.Dtos;
using CapShop.PaymentService.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace CapShop.PaymentService.Controllers;

[ApiController]
[Route("payments")]
[Authorize]
public class PaymentController : ControllerBase
{
    private readonly IPaymentService _payments;

    public PaymentController(IPaymentService payments)
    {
        _payments = payments;
    }

    private Guid GetUserId()
    {
        var sub = User.FindFirstValue(ClaimTypes.NameIdentifier)
                  ?? User.FindFirstValue("sub");
        return Guid.Parse(sub!);
    }

    [HttpGet("health")]
    [AllowAnonymous]
    public IActionResult Health() =>
        Ok(new { service = "PaymentService", status = "Healthy" });

    [HttpPost("simulate")]
    public async Task<IActionResult> Simulate([FromBody] SimulatePaymentRequest request)
    {
        var result = await _payments.SimulateAsync(GetUserId(), request);
        return Ok(result);
    }

    [HttpGet("order/{orderId:guid}")]
    public async Task<IActionResult> GetByOrder(Guid orderId)
    {
        var result = await _payments.GetByOrderIdAsync(orderId);
        if (result is null)
            return NotFound(new { message = "No transaction found for this order." });

        return Ok(result);
    }
}

using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using CapShop.AdminService.Dtos;
using CapShop.AdminService.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace CapShop.AdminService.Controllers;

[ApiController]
[Route("admin/orders")]
[Authorize(Roles = "Admin")]
public class AdminOrderController : ControllerBase
{
    private readonly IOrderStatusService _orders;

    public AdminOrderController(IOrderStatusService orders)
    {
        _orders = orders;
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
        var result = await _orders.GetAllOrdersAsync();
        return Ok(result);
    }

    [HttpPut("{id:guid}/status")]
    public async Task<IActionResult> UpdateStatus(Guid id, [FromBody] UpdateOrderStatusRequest request)
    {
        var result = await _orders.UpdateOrderStatusAsync(GetAdminUserId(), id, request);
        return Ok(result);
    }
}
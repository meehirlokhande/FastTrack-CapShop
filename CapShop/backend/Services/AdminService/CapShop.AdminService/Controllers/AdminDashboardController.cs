using CapShop.AdminService.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace CapShop.AdminService.Controllers;

[ApiController]
[Route("admin/dashboard")]
[Authorize(Roles = "Admin")]
public class AdminDashboardController : ControllerBase
{
    private readonly IDashboardService _dashboard;

    public AdminDashboardController(IDashboardService dashboard)
    {
        _dashboard = dashboard;
    }

    [HttpGet("health")]
    [AllowAnonymous]
    public IActionResult Health()
    {
        return Ok(new { service = "AdminService", status = "Healthy" });
    }

    [HttpGet("summary")]
    public async Task<IActionResult> GetSummary()
    {
        var result = await _dashboard.GetSummaryAsync();
        return Ok(result);
    }
}
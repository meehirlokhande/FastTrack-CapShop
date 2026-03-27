using CapShop.AdminService.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace CapShop.AdminService.Controllers;

[ApiController]
[Route("admin/reports")]
[Authorize(Roles = "Admin")]
public class AdminReportController : ControllerBase
{
    private readonly IReportService _reports;

    public AdminReportController(IReportService reports)
    {
        _reports = reports;
    }

    [HttpGet("sales")]
    public async Task<IActionResult> GetSalesReport([FromQuery] DateTime from, [FromQuery] DateTime to)
    {
        var result = await _reports.GetSalesReportAsync(from, to);
        return Ok(result);
    }

    [HttpGet("status-split")]
    public async Task<IActionResult> GetStatusSplit()
    {
        var result = await _reports.GetStatusSplitAsync();
        return Ok(result);
    }
}
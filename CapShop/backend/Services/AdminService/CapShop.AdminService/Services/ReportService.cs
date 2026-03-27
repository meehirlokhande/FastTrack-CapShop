using CapShop.AdminService.Dtos;
using CapShop.AdminService.Models;
using CapShop.AdminService.Repositories;

namespace CapShop.AdminService.Services;

public class ReportService : IReportService
{
    private readonly IAdminOrderRepository _orders;

    public ReportService(IAdminOrderRepository orders)
    {
        _orders = orders;
    }

    public async Task<List<SalesReportResponse>> GetSalesReportAsync(DateTime from, DateTime to)
    {
        var orders = await _orders.GetByDateRangeAsync(from, to);

        return orders
            .GroupBy(o => o.CreatedAt.Date)
            .Select(g => new SalesReportResponse
            {
                Date = g.Key,
                OrderCount = g.Count(),
                Revenue = g.Sum(o => o.TotalAmount)
            })
            .OrderBy(r => r.Date)
            .ToList();
    }

    public async Task<List<StatusSplitResponse>> GetStatusSplitAsync()
    {
        var allOrders = await _orders.GetAllAsync();

        return allOrders
            .GroupBy(o => o.Status)
            .Select(g => new StatusSplitResponse
            {
                Status = g.Key.ToString(),
                Count = g.Count()
            })
            .OrderBy(s => s.Status)
            .ToList();
    }
}
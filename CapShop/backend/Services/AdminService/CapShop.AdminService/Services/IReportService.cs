using CapShop.AdminService.Dtos;

namespace CapShop.AdminService.Services;

public interface IReportService
{
    Task<List<SalesReportResponse>> GetSalesReportAsync(DateTime from, DateTime to);
    Task<List<StatusSplitResponse>> GetStatusSplitAsync();
}
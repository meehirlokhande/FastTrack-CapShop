using CapShop.AdminService.Dtos;

namespace CapShop.AdminService.Services;

public interface IDashboardService
{
    Task<DashboardSummaryResponse> GetSummaryAsync();
}
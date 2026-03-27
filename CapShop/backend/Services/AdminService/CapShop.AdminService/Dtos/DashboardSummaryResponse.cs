namespace CapShop.AdminService.Dtos;

public class DashboardSummaryResponse
{
    public int TotalProducts { get; set; }
    public int TotalOrders { get; set; }
    public decimal TotalRevenue { get; set; }
    public int PendingOrders { get; set; }
    public int DeliveredOrders { get; set; }
    public List<AdminOrderResponse> RecentOrders { get; set; } = new();
}
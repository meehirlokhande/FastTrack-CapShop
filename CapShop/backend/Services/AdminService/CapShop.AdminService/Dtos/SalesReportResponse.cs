namespace CapShop.AdminService.Dtos;

public class SalesReportResponse
{
    public DateTime Date { get; set; }
    public int OrderCount { get; set; }
    public decimal Revenue { get; set; }
}
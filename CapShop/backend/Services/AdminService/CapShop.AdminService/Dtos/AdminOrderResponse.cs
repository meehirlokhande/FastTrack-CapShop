namespace CapShop.AdminService.Dtos;

public class AdminOrderResponse
{
    public Guid Id { get; set; }
    public Guid UserId { get; set; }
    public string Status { get; set; } = string.Empty;
    public decimal TotalAmount { get; set; }
    public string ShippingCity { get; set; } = string.Empty;
    public int ItemCount { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? PaidAt { get; set; }
}
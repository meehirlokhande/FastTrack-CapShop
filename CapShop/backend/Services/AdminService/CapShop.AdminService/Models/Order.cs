namespace CapShop.AdminService.Models;

public class Order
{
    public Guid Id { get; set; }
    public Guid UserId { get; set; }
    public OrderStatus Status { get; set; }
    public decimal TotalAmount { get; set; }
    public int? PaymentMethod { get; set; }
    public string ShippingAddress { get; set; } = string.Empty;
    public string ShippingCity { get; set; } = string.Empty;
    public string ShippingPincode { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
    public DateTime? PaidAt { get; set; }

    public ICollection<OrderItem> Items { get; set; } = new List<OrderItem>();
}
namespace CapShop.OrderService.Dtos;

public class PaymentRequest
{
    public Guid OrderId { get; set; }
    public string PaymentMethod { get; set; } = string.Empty;
}
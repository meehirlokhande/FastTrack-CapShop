namespace CapShop.OrderService.Dtos;

public class UpdatePaymentStatusRequest
{
    public Guid UserId { get; set; }
    public string Status { get; set; } = string.Empty;
    public string PaymentMethod { get; set; } = string.Empty;
    public DateTime? PaidAt { get; set; }
}

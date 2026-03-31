namespace CapShop.PaymentService.Dtos;

public class PaymentResponse
{
    public Guid TransactionId { get; set; }
    public Guid OrderId { get; set; }
    public string Status { get; set; } = string.Empty;
    public string PaymentMethod { get; set; } = string.Empty;
    public decimal Amount { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? CompletedAt { get; set; }
}

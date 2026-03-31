namespace CapShop.PaymentService.Services;

public interface IOrderHttpClient
{
    Task UpdatePaymentStatusAsync(
        Guid orderId,
        Guid userId,
        string status,
        string paymentMethod,
        DateTime? paidAt);
}

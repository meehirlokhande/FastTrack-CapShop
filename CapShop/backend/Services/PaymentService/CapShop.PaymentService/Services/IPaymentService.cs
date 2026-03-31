using CapShop.PaymentService.Dtos;

namespace CapShop.PaymentService.Services;

public interface IPaymentService
{
    Task<PaymentResponse> SimulateAsync(Guid userId, SimulatePaymentRequest request);
    Task<PaymentResponse?> GetByOrderIdAsync(Guid orderId);
}

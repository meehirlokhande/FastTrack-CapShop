using CapShop.PaymentService.Dtos;
using CapShop.PaymentService.Models;
using CapShop.PaymentService.Repositories;

namespace CapShop.PaymentService.Services;

public class PaymentServiceImpl : IPaymentService
{
    private readonly ITransactionRepository _transactions;
    private readonly IOrderHttpClient _orderClient;
    private readonly ILogger<PaymentServiceImpl> _logger;

    public PaymentServiceImpl(
        ITransactionRepository transactions,
        IOrderHttpClient orderClient,
        ILogger<PaymentServiceImpl> logger)
    {
        _transactions = transactions;
        _orderClient = orderClient;
        _logger = logger;
    }

    public async Task<PaymentResponse> SimulateAsync(Guid userId, SimulatePaymentRequest request)
    {
        if (!Enum.TryParse<PaymentMethod>(request.PaymentMethod, ignoreCase: true, out var method))
            throw new ArgumentException("Invalid payment method. Use UPI, Card, or COD.");

        if (request.Amount <= 0)
            throw new ArgumentException("Payment amount must be greater than zero.");

        var transaction = new Transaction
        {
            Id = Guid.NewGuid(),
            OrderId = request.OrderId,
            UserId = userId,
            Amount = request.Amount,
            Method = method,
            Status = TransactionStatus.Pending,
            CreatedAt = DateTime.UtcNow
        };

        await _transactions.AddAsync(transaction);

        // COD always succeeds; UPI and Card simulate 90% success rate
        var success = method == PaymentMethod.COD || Random.Shared.Next(100) < 90;

        transaction.Status = success ? TransactionStatus.Captured : TransactionStatus.Failed;
        transaction.CompletedAt = DateTime.UtcNow;
        await _transactions.UpdateAsync(transaction);

        var orderStatus = success ? "Paid" : "PaymentFailed";

        await _orderClient.UpdatePaymentStatusAsync(
            request.OrderId, userId, orderStatus, method.ToString(), transaction.CompletedAt);

        _logger.LogInformation(
            "Payment {Status} for order {OrderId}, method: {Method}, transaction: {TxId}",
            orderStatus, request.OrderId, method, transaction.Id);

        return MapToResponse(transaction);
    }

    public async Task<PaymentResponse?> GetByOrderIdAsync(Guid orderId)
    {
        var transaction = await _transactions.GetByOrderIdAsync(orderId);
        return transaction is null ? null : MapToResponse(transaction);
    }

    private static PaymentResponse MapToResponse(Transaction t) => new()
    {
        TransactionId = t.Id,
        OrderId = t.OrderId,
        Status = t.Status.ToString(),
        PaymentMethod = t.Method.ToString(),
        Amount = t.Amount,
        CreatedAt = t.CreatedAt,
        CompletedAt = t.CompletedAt
    };
}

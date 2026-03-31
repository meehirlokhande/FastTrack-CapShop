using CapShop.PaymentService.Models;

namespace CapShop.PaymentService.Repositories;

public interface ITransactionRepository
{
    Task AddAsync(Transaction transaction);
    Task UpdateAsync(Transaction transaction);
    Task<Transaction?> GetByIdAsync(Guid id);
    Task<Transaction?> GetByOrderIdAsync(Guid orderId);
}

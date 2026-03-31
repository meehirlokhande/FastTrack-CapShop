using CapShop.PaymentService.Data;
using CapShop.PaymentService.Models;
using Microsoft.EntityFrameworkCore;

namespace CapShop.PaymentService.Repositories;

public class TransactionRepository : ITransactionRepository
{
    private readonly PaymentDbContext _db;

    public TransactionRepository(PaymentDbContext db)
    {
        _db = db;
    }

    public async Task AddAsync(Transaction transaction)
    {
        _db.Transactions.Add(transaction);
        await _db.SaveChangesAsync();
    }

    public async Task UpdateAsync(Transaction transaction)
    {
        _db.Transactions.Update(transaction);
        await _db.SaveChangesAsync();
    }

    public async Task<Transaction?> GetByIdAsync(Guid id)
    {
        return await _db.Transactions.FindAsync(id);
    }

    public async Task<Transaction?> GetByOrderIdAsync(Guid orderId)
    {
        return await _db.Transactions
            .Where(t => t.OrderId == orderId)
            .OrderByDescending(t => t.CreatedAt)
            .FirstOrDefaultAsync();
    }
}

using CapShop.AuthService.Data;
using CapShop.AuthService.Models;
using Microsoft.EntityFrameworkCore;

namespace CapShop.AuthService.Repositories;

public class OtpCodeRepository : IOtpCodeRepository
{
    private readonly AuthDbContext _db;

    public OtpCodeRepository(AuthDbContext db)
    {
        _db = db;
    }

    public async Task AddAsync(OtpCode otpCode)
    {
        _db.OtpCodes.Add(otpCode);
        await _db.SaveChangesAsync();
    }

    public async Task<OtpCode?> FindValidAsync(Guid userId, string codeHash)
    {
        return await _db.OtpCodes
            .Where(x => x.UserId == userId
                     && x.CodeHash == codeHash
                     && !x.IsUsed
                     && x.ExpiresAt > DateTime.UtcNow)
            .OrderByDescending(x => x.CreatedAt)
            .FirstOrDefaultAsync();
    }

    public async Task MarkUsedAsync(OtpCode otpCode)
    {
        otpCode.IsUsed = true;
        await _db.SaveChangesAsync();
    }

    public async Task DeleteExpiredAsync(Guid userId)
    {
        var expired = await _db.OtpCodes
            .Where(x => x.UserId == userId && (x.IsUsed || x.ExpiresAt <= DateTime.UtcNow))
            .ToListAsync();

        _db.OtpCodes.RemoveRange(expired);
        await _db.SaveChangesAsync();
    }
}

using CapShop.AuthService.Models;

namespace CapShop.AuthService.Repositories;

public interface IOtpCodeRepository
{
    Task AddAsync(OtpCode otpCode);
    Task<OtpCode?> FindValidAsync(Guid userId, string codeHash);
    Task MarkUsedAsync(OtpCode otpCode);
    Task DeleteExpiredAsync(Guid userId);
}

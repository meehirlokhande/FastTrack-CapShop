using CapShop.AuthService.Dtos;
using CapShop.AuthService.Models;

namespace CapShop.AuthService.Services;

public interface ITwoFactorService
{
    Task<TotpSetupResponse> SetupTotpAsync(Guid userId, string email);
    Task EnableTotpAsync(Guid userId, string code);

    Task SendOtpAsync(Guid userId, TwoFactorMethod method);
    Task EnableOtpMethodAsync(Guid userId, string code, TwoFactorMethod method);

    Task<bool> VerifyTotpCodeAsync(Guid userId, string code);
    Task<bool> VerifyOtpCodeAsync(Guid userId, string code);

    Task DisableAsync(Guid userId);
    Task<TwoFactorStatusResponse> GetStatusAsync(Guid userId);
}

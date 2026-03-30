using CapShop.AuthService.Dtos;

namespace CapShop.AuthService.Services;

public interface IAccountService
{
    Task<string> SignupAsync(SignUpRequest request);
    Task<AuthResponse> LoginAsync(LoginRequest request);
    Task<AuthResponse> VerifyTwoFactorAsync(VerifyTwoFactorRequest request);
    Task ResendTwoFactorCodeAsync(string tempToken);
}
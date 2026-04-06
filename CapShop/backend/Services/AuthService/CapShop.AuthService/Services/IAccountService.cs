using CapShop.AuthService.Dtos;

namespace CapShop.AuthService.Services;

public interface IAccountService
{
    Task<string> SignupAsync(SignUpRequest request);
    Task<AuthResponse> LoginAsync(LoginRequest request);
    Task<AuthResponse> VerifyTwoFactorAsync(VerifyTwoFactorRequest request);
    Task ResendTwoFactorCodeAsync(string tempToken);

    Task<ProfileResponse> GetProfileAsync(Guid userId);
    Task<ProfileResponse> UpdateProfileAsync(Guid userId, UpdateProfileRequest request);
    Task ChangePasswordAsync(Guid userId, ChangePasswordRequest request);
    Task<ProfileResponse> SetAvatarAsync(Guid userId, Stream fileStream, string contentType, long length);
    Task<ProfileResponse> RemoveAvatarAsync(Guid userId);
}
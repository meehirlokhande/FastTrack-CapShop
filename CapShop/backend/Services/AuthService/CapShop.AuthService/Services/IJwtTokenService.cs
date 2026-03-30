using CapShop.AuthService.Models;

namespace CapShop.AuthService.Services;

public interface IJwtTokenService
{
    string GenerateToken(User user);
    string GenerateTempToken(Guid userId, TwoFactorMethod method);
    TempTokenPayload? ValidateTempToken(string tempToken);
}

public record TempTokenPayload(Guid UserId, TwoFactorMethod Method);
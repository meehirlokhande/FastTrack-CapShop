using CapShop.AuthService.Models;

namespace CapShop.AuthService.Services;

public interface IJwtTokenService
{
    string GenerateToken(User user);
}
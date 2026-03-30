namespace CapShop.AuthService.Dtos;

public class AuthResponse
{
    // Populated when login succeeds without 2FA, or after 2FA verification
    public string? Token { get; set; }
    public string? Role { get; set; }

    // Populated when 2FA is required
    public bool RequiresTwoFactor { get; set; }
    public string? TempToken { get; set; }
    public string? TwoFactorMethod { get; set; }
}
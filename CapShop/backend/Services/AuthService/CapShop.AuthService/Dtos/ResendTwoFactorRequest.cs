namespace CapShop.AuthService.Dtos;

public class ResendTwoFactorRequest
{
    public string TempToken { get; set; } = string.Empty;
}

namespace CapShop.AuthService.Dtos;

public class TwoFactorStatusResponse
{
    public bool Enabled { get; set; }
    public string Method { get; set; } = "None";
}

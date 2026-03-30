namespace CapShop.AuthService.Dtos;

public class TotpSetupResponse
{
    public string Secret { get; set; } = string.Empty;
    public string QrCodeUri { get; set; } = string.Empty;
}

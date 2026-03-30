namespace CapShop.AuthService.Services;

public interface INotificationService
{
    Task SendEmailOtpAsync(string toEmail, string code);
    Task SendSmsOtpAsync(string phoneNumber, string code);
}

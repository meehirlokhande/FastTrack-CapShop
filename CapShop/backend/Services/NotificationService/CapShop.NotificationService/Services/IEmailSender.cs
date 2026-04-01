namespace CapShop.NotificationService.Services;

public interface IEmailSender
{
    Task SendAsync(string toEmail, string toName, string subject, string htmlBody);
}

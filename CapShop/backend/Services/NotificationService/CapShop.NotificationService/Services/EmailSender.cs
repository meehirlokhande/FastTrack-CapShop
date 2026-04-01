using MailKit.Net.Smtp;
using MailKit.Security;
using MimeKit;

namespace CapShop.NotificationService.Services;

public class EmailSender : IEmailSender
{
    private readonly IConfiguration _config;
    private readonly ILogger<EmailSender> _logger;

    public EmailSender(IConfiguration config, ILogger<EmailSender> logger)
    {
        _config = config;
        _logger = logger;
    }

    public async Task SendAsync(string toEmail, string toName, string subject, string htmlBody)
    {
        var smtp = _config.GetSection("Smtp");

        var message = new MimeMessage();
        message.From.Add(new MailboxAddress(
            smtp["FromName"] ?? "CapShop",
            smtp["FromEmail"] ?? throw new InvalidOperationException("Smtp:FromEmail is missing")));
        message.To.Add(new MailboxAddress(toName, toEmail));
        message.Subject = subject;
        message.Body = new TextPart("html") { Text = htmlBody };

        using var client = new SmtpClient();
        await client.ConnectAsync(
            smtp["Host"] ?? throw new InvalidOperationException("Smtp:Host is missing"),
            int.Parse(smtp["Port"] ?? "587"),
            SecureSocketOptions.StartTls);

        await client.AuthenticateAsync(
            smtp["Username"] ?? throw new InvalidOperationException("Smtp:Username is missing"),
            smtp["Password"] ?? throw new InvalidOperationException("Smtp:Password is missing"));

        await client.SendAsync(message);
        await client.DisconnectAsync(true);

        _logger.LogInformation("Notification email sent to {Email}, subject: {Subject}", toEmail, subject);
    }
}

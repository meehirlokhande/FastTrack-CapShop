using MailKit.Net.Smtp;
using MailKit.Security;
using MimeKit;
using Twilio;
using Twilio.Rest.Api.V2010.Account;

namespace CapShop.AuthService.Services;

public class NotificationService : INotificationService
{
    private readonly IConfiguration _config;
    private readonly ILogger<NotificationService> _logger;

    public NotificationService(IConfiguration config, ILogger<NotificationService> logger)
    {
        _config = config;
        _logger = logger;
    }

    public async Task SendEmailOtpAsync(string toEmail, string code)
    {
        var smtp = _config.GetSection("Smtp");

        var message = new MimeMessage();
        message.From.Add(new MailboxAddress(
            smtp["FromName"] ?? "CapShop",
            smtp["FromEmail"] ?? throw new InvalidOperationException("Smtp:FromEmail is missing")));
        message.To.Add(MailboxAddress.Parse(toEmail));
        message.Subject = "Your CapShop verification code";

        message.Body = new TextPart("html")
        {
            Text = $"""
                    <div style="font-family:sans-serif;max-width:480px;margin:0 auto;">
                      <h2 style="color:#4f46e5;">CapShop Verification</h2>
                      <p>Your one-time verification code is:</p>
                      <p style="font-size:36px;font-weight:bold;letter-spacing:8px;color:#111;">{code}</p>
                      <p style="color:#666;">This code expires in 5 minutes. Do not share it with anyone.</p>
                    </div>
                    """
        };

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

        _logger.LogInformation("OTP email sent to {Email}", toEmail);
    }

    public async Task SendSmsOtpAsync(string phoneNumber, string code)
    {
        var twilio = _config.GetSection("Twilio");

        var accountSid = twilio["AccountSid"] ?? throw new InvalidOperationException("Twilio:AccountSid is missing");
        var authToken = twilio["AuthToken"] ?? throw new InvalidOperationException("Twilio:AuthToken is missing");
        var fromNumber = twilio["FromNumber"] ?? throw new InvalidOperationException("Twilio:FromNumber is missing");

        TwilioClient.Init(accountSid, authToken);

        await MessageResource.CreateAsync(
            body: $"Your CapShop verification code is {code}. Expires in 5 minutes.",
            from: new Twilio.Types.PhoneNumber(fromNumber),
            to: new Twilio.Types.PhoneNumber(phoneNumber));

        _logger.LogInformation("OTP SMS sent to {Phone}", phoneNumber);
    }
}

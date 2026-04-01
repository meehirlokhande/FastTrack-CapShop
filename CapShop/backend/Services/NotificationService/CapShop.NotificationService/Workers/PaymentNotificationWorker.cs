using System.Text;
using System.Text.Json;
using CapShop.NotificationService.Services;
using CapShop.Shared.Events;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;

namespace CapShop.NotificationService.Workers;

public class PaymentNotificationWorker : BackgroundService
{
    private readonly IConnection _connection;
    private readonly IServiceProvider _services;
    private readonly ILogger<PaymentNotificationWorker> _logger;

    public PaymentNotificationWorker(
        IConnection connection,
        IServiceProvider services,
        ILogger<PaymentNotificationWorker> logger)
    {
        _connection = connection;
        _services = services;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        var channel = await _connection.CreateChannelAsync(cancellationToken: stoppingToken);

        await channel.QueueDeclareAsync(
            queue: QueueNames.PaymentCompleted,
            durable: true,
            exclusive: false,
            autoDelete: false,
            arguments: null,
            cancellationToken: stoppingToken);

        await channel.BasicQosAsync(prefetchSize: 0, prefetchCount: 1, global: false, cancellationToken: stoppingToken);

        var consumer = new AsyncEventingBasicConsumer(channel);

        consumer.ReceivedAsync += async (_, ea) =>
        {
            try
            {
                var json = Encoding.UTF8.GetString(ea.Body.Span);
                var evt = JsonSerializer.Deserialize<PaymentCompletedEvent>(json);

                if (evt is not null)
                    await HandleAsync(evt);

                await channel.BasicAckAsync(ea.DeliveryTag, multiple: false);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error processing PaymentCompletedEvent, nacking message");
                await channel.BasicNackAsync(ea.DeliveryTag, multiple: false, requeue: false);
            }
        };

        await channel.BasicConsumeAsync(
            queue: QueueNames.PaymentCompleted,
            autoAck: false,
            consumer: consumer,
            cancellationToken: stoppingToken);

        await Task.Delay(Timeout.Infinite, stoppingToken);
    }

    private async Task HandleAsync(PaymentCompletedEvent evt)
    {
        using var scope = _services.CreateScope();
        var authClient = scope.ServiceProvider.GetRequiredService<IAuthHttpClient>();
        var emailSender = scope.ServiceProvider.GetRequiredService<IEmailSender>();

        var user = await authClient.GetUserAsync(evt.UserId);
        if (user is null)
        {
            _logger.LogWarning("Could not resolve user {UserId} for payment notification", evt.UserId);
            return;
        }

        var (subject, body) = evt.Status == "Captured"
            ? BuildCapturedEmail(user.FullName, evt)
            : BuildFailedEmail(user.FullName, evt);

        await emailSender.SendAsync(user.Email, user.FullName, subject, body);
    }

    private static (string subject, string body) BuildCapturedEmail(string name, PaymentCompletedEvent evt) => (
        "Your CapShop order is confirmed!",
        $"""
        <div style="font-family:sans-serif;max-width:520px;margin:0 auto;">
          <h2 style="color:#16a34a;">Order Confirmed</h2>
          <p>Hi {name},</p>
          <p>Your payment of <strong>₹{evt.Amount:N2}</strong> via <strong>{evt.PaymentMethod}</strong> was successful.</p>
          <p>Order ID: <code>{evt.OrderId}</code></p>
          <p style="color:#555;">We're preparing your order. You'll receive another update when it ships.</p>
        </div>
        """);

    private static (string subject, string body) BuildFailedEmail(string name, PaymentCompletedEvent evt) => (
        "Your CapShop payment failed",
        $"""
        <div style="font-family:sans-serif;max-width:520px;margin:0 auto;">
          <h2 style="color:#dc2626;">Payment Failed</h2>
          <p>Hi {name},</p>
          <p>Your payment of <strong>₹{evt.Amount:N2}</strong> via <strong>{evt.PaymentMethod}</strong> could not be processed.</p>
          <p>Order ID: <code>{evt.OrderId}</code></p>
          <p>Please retry with a different payment method.</p>
        </div>
        """);
}

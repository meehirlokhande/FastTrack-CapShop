using System.Text;
using System.Text.Json;
using CapShop.NotificationService.Services;
using CapShop.Shared.Events;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;

namespace CapShop.NotificationService.Workers;

public class OrderNotificationWorker : BackgroundService
{
    private readonly IConnection _connection;
    private readonly IServiceProvider _services;
    private readonly ILogger<OrderNotificationWorker> _logger;

    public OrderNotificationWorker(
        IConnection connection,
        IServiceProvider services,
        ILogger<OrderNotificationWorker> logger)
    {
        _connection = connection;
        _services = services;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        var channel = await _connection.CreateChannelAsync(cancellationToken: stoppingToken);

        await channel.QueueDeclareAsync(
            queue: QueueNames.OrderCancelled,
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
                var evt = JsonSerializer.Deserialize<OrderCancelledEvent>(json);

                if (evt is not null)
                    await HandleAsync(evt);

                await channel.BasicAckAsync(ea.DeliveryTag, multiple: false);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error processing OrderCancelledEvent, nacking message");
                await channel.BasicNackAsync(ea.DeliveryTag, multiple: false, requeue: false);
            }
        };

        await channel.BasicConsumeAsync(
            queue: QueueNames.OrderCancelled,
            autoAck: false,
            consumer: consumer,
            cancellationToken: stoppingToken);

        await Task.Delay(Timeout.Infinite, stoppingToken);
    }

    private async Task HandleAsync(OrderCancelledEvent evt)
    {
        using var scope = _services.CreateScope();
        var authClient = scope.ServiceProvider.GetRequiredService<IAuthHttpClient>();
        var emailSender = scope.ServiceProvider.GetRequiredService<IEmailSender>();

        var user = await authClient.GetUserAsync(evt.UserId);
        if (user is null)
        {
            _logger.LogWarning("Could not resolve user {UserId} for cancellation notification", evt.UserId);
            return;
        }

        var subject = "Your CapShop order has been cancelled";
        var body = $"""
            <div style="font-family:sans-serif;max-width:520px;margin:0 auto;">
              <h2 style="color:#f59e0b;">Order Cancelled</h2>
              <p>Hi {user.FullName},</p>
              <p>Your order <code>{evt.OrderId}</code> worth <strong>₹{evt.TotalAmount:N2}</strong> has been cancelled.</p>
              <p style="color:#555;">If this was a mistake, you can place a new order anytime.</p>
            </div>
            """;

        await emailSender.SendAsync(user.Email, user.FullName, subject, body);
    }
}

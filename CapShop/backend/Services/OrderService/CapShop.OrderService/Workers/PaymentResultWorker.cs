using System.Text;
using System.Text.Json;
using CapShop.OrderService.Services;
using CapShop.Shared.Events;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;

namespace CapShop.OrderService.Workers;

public class PaymentResultWorker : BackgroundService
{
    // Each consumer binds its own durable queue to the shared fanout exchange.
    private const string ConsumerQueue = "capshop.payment.completed.orders";

    private readonly IConnection _connection;
    private readonly IServiceProvider _services;
    private readonly ILogger<PaymentResultWorker> _logger;

    public PaymentResultWorker(
        IConnection connection,
        IServiceProvider services,
        ILogger<PaymentResultWorker> logger)
    {
        _connection = connection;
        _services = services;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        var channel = await _connection.CreateChannelAsync(cancellationToken: stoppingToken);

        await channel.ExchangeDeclareAsync(
            exchange: QueueNames.PaymentCompleted,
            type: ExchangeType.Fanout,
            durable: true,
            autoDelete: false,
            arguments: null,
            cancellationToken: stoppingToken);

        await channel.QueueDeclareAsync(
            queue: ConsumerQueue,
            durable: true,
            exclusive: false,
            autoDelete: false,
            arguments: null,
            cancellationToken: stoppingToken);

        await channel.QueueBindAsync(
            queue: ConsumerQueue,
            exchange: QueueNames.PaymentCompleted,
            routingKey: string.Empty,
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
            queue: ConsumerQueue,
            autoAck: false,
            consumer: consumer,
            cancellationToken: stoppingToken);

        await Task.Delay(Timeout.Infinite, stoppingToken);
    }

    private async Task HandleAsync(PaymentCompletedEvent evt)
    {
        using var scope = _services.CreateScope();
        var orderService = scope.ServiceProvider.GetRequiredService<IOrderManagementService>();

        var paidAt = evt.Status == "Captured" ? evt.CompletedAt : (DateTime?)null;

        try
        {
            await orderService.UpdatePaymentStatusAsync(
                orderId: evt.OrderId,
                userId: evt.UserId,
                status: evt.Status == "Captured" ? "Paid" : "PaymentFailed",
                paymentMethod: evt.PaymentMethod,
                paidAt: paidAt);

            _logger.LogInformation(
                "[{CorrelationId}] Order {OrderId} status updated to {Status} via PaymentCompletedEvent",
                evt.CorrelationId, evt.OrderId, evt.Status);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "[{CorrelationId}] Failed to update order {OrderId} from PaymentCompletedEvent",
                evt.CorrelationId, evt.OrderId);
            throw; // rethrow so BasicNack is called by the outer catch
        }
    }
}

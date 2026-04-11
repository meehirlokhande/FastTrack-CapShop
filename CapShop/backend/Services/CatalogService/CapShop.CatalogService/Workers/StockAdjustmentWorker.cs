using System.Text;
using System.Text.Json;
using CapShop.CatalogService.Services;
using CapShop.Shared.Events;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using CatalogStockAdjustItem = CapShop.CatalogService.Dtos.StockAdjustItem;

namespace CapShop.CatalogService.Workers;

public class StockAdjustmentWorker : BackgroundService
{
    private const string ConsumerQueue = "capshop.stock.adjustment.requested.catalog";

    private readonly IConnection _connection;
    private readonly IServiceProvider _services;
    private readonly ILogger<StockAdjustmentWorker> _logger;

    public StockAdjustmentWorker(
        IConnection connection,
        IServiceProvider services,
        ILogger<StockAdjustmentWorker> logger)
    {
        _connection = connection;
        _services = services;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        var channel = await _connection.CreateChannelAsync(cancellationToken: stoppingToken);

        await channel.ExchangeDeclareAsync(
            exchange: QueueNames.StockAdjustmentRequested,
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
            exchange: QueueNames.StockAdjustmentRequested,
            routingKey: string.Empty,
            cancellationToken: stoppingToken);

        await channel.BasicQosAsync(prefetchSize: 0, prefetchCount: 1, global: false, cancellationToken: stoppingToken);

        var consumer = new AsyncEventingBasicConsumer(channel);

        consumer.ReceivedAsync += async (_, ea) =>
        {
            try
            {
                var json = Encoding.UTF8.GetString(ea.Body.Span);
                var evt = JsonSerializer.Deserialize<StockAdjustmentRequestedEvent>(json);

                if (evt is not null)
                    await HandleAsync(evt);

                await channel.BasicAckAsync(ea.DeliveryTag, multiple: false);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error processing StockAdjustmentRequestedEvent, nacking message");
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

    private async Task HandleAsync(StockAdjustmentRequestedEvent evt)
    {
        using var scope = _services.CreateScope();
        var catalogService = scope.ServiceProvider.GetRequiredService<ICatalogService>();

        var items = evt.Items
            .Select(i => new CatalogStockAdjustItem(i.ProductId, i.Delta))
            .ToList();

        await catalogService.AdjustStockBatchAsync(items);

        _logger.LogInformation(
            "[{CorrelationId}] Stock adjusted for order {OrderId}: {Count} product(s)",
            evt.CorrelationId, evt.OrderId, items.Count);
    }
}

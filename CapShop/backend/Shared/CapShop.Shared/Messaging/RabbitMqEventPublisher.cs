using System.Text.Json;
using Microsoft.Extensions.Logging;
using RabbitMQ.Client;

namespace CapShop.Shared.Messaging;

public class RabbitMqEventPublisher : IEventPublisher
{
    private readonly IConnection _connection;
    private readonly ILogger<RabbitMqEventPublisher> _logger;

    public RabbitMqEventPublisher(IConnection connection, ILogger<RabbitMqEventPublisher> logger)
    {
        _connection = connection;
        _logger = logger;
    }

    public async Task PublishAsync<T>(string exchange, T message)
    {
        try
        {
            await using var channel = await _connection.CreateChannelAsync();

            await channel.ExchangeDeclareAsync(
                exchange: exchange,
                type: ExchangeType.Fanout,
                durable: true,
                autoDelete: false,
                arguments: null);

            var body = JsonSerializer.SerializeToUtf8Bytes(message);
            var props = new BasicProperties { Persistent = true };

            await channel.BasicPublishAsync(
                exchange: exchange,
                routingKey: string.Empty,
                mandatory: false,
                basicProperties: props,
                body: body);

            var correlationId = message is CapShop.Shared.Events.ICorrelatedEvent e ? e.CorrelationId : "-";
            _logger.LogInformation("Published {EventType} to exchange {Exchange} [{CorrelationId}]",
                typeof(T).Name, exchange, correlationId);
        }
        catch (Exception ex)
        {
            // Messaging failure should not roll back the primary transaction
            _logger.LogError(ex, "Failed to publish {EventType} to exchange {Exchange}", typeof(T).Name, exchange);
        }
    }
}

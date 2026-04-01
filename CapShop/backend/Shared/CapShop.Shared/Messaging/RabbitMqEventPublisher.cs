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

    public async Task PublishAsync<T>(string queue, T message)
    {
        try
        {
            await using var channel = await _connection.CreateChannelAsync();

            await channel.QueueDeclareAsync(
                queue: queue,
                durable: true,
                exclusive: false,
                autoDelete: false,
                arguments: null);

            var body = JsonSerializer.SerializeToUtf8Bytes(message);
            var props = new BasicProperties { Persistent = true };

            await channel.BasicPublishAsync(
                exchange: string.Empty,
                routingKey: queue,
                mandatory: false,
                basicProperties: props,
                body: body);

            _logger.LogInformation("Published {EventType} to queue {Queue}", typeof(T).Name, queue);
        }
        catch (Exception ex)
        {
            // Messaging failure should not roll back the primary transaction
            _logger.LogError(ex, "Failed to publish {EventType} to queue {Queue}", typeof(T).Name, queue);
        }
    }
}

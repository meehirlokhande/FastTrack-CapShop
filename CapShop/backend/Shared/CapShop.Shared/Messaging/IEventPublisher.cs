namespace CapShop.Shared.Messaging;

public interface IEventPublisher
{
    Task PublishAsync<T>(string queue, T message);
}

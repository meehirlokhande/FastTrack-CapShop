namespace CapShop.Shared.Events;

/// <summary>Marker so the publisher can extract CorrelationId without knowing the concrete event type.</summary>
public interface ICorrelatedEvent
{
    string CorrelationId { get; }
}

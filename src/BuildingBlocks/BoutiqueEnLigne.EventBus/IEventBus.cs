namespace BoutiqueEnLigne.EventBus;

public interface IEventBus
{
    Task PublishAsync(
        string eventName,
        Guid messageId,
        string jsonPayload,
        CancellationToken cancellationToken = default);
}

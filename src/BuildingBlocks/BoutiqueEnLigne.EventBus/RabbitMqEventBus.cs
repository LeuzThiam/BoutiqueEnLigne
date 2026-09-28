using System.Text;
using Microsoft.Extensions.Logging;
using RabbitMQ.Client;

namespace BoutiqueEnLigne.EventBus;

public sealed class RabbitMqEventBus : IEventBus, IAsyncDisposable
{
    private readonly RabbitMqOptions _options;
    private readonly ILogger<RabbitMqEventBus> _logger;
    private readonly SemaphoreSlim _connectionLock = new(1, 1);
    private IConnection? _connection;

    public RabbitMqEventBus(RabbitMqOptions options, ILogger<RabbitMqEventBus> logger)
    {
        _options = options;
        _logger = logger;
    }

    public async Task PublishAsync(
        string eventName,
        Guid messageId,
        string jsonPayload,
        CancellationToken cancellationToken = default)
    {
        var connection = await GetConnectionAsync(cancellationToken);
        await using var channel = await connection.CreateChannelAsync(
            new CreateChannelOptions(true, true), cancellationToken);

        await channel.ExchangeDeclareAsync(
            _options.ExchangeName,
            ExchangeType.Topic,
            durable: true,
            autoDelete: false,
            cancellationToken: cancellationToken);

        var properties = new BasicProperties
        {
            ContentType = "application/json",
            DeliveryMode = DeliveryModes.Persistent,
            MessageId = messageId.ToString(),
            Type = eventName,
            Timestamp = new AmqpTimestamp(DateTimeOffset.UtcNow.ToUnixTimeSeconds())
        };

        await channel.BasicPublishAsync(
            _options.ExchangeName,
            eventName,
            mandatory: true,
            basicProperties: properties,
            body: Encoding.UTF8.GetBytes(jsonPayload),
            cancellationToken: cancellationToken);

        _logger.LogInformation("Published integration event {EventName} ({MessageId})", eventName, messageId);
    }

    private async Task<IConnection> GetConnectionAsync(CancellationToken cancellationToken)
    {
        if (_connection is { IsOpen: true }) return _connection;

        await _connectionLock.WaitAsync(cancellationToken);
        try
        {
            if (_connection is { IsOpen: true }) return _connection;

            if (_connection is not null) await _connection.DisposeAsync();
            var factory = new ConnectionFactory
            {
                HostName = _options.Host,
                Port = _options.Port,
                UserName = _options.UserName,
                Password = _options.Password,
                VirtualHost = _options.VirtualHost,
                AutomaticRecoveryEnabled = true,
                TopologyRecoveryEnabled = true,
                ClientProvidedName = "boutique-event-bus"
            };
            _connection = await factory.CreateConnectionAsync(cancellationToken);
            return _connection;
        }
        finally
        {
            _connectionLock.Release();
        }
    }

    public async ValueTask DisposeAsync()
    {
        if (_connection is not null) await _connection.DisposeAsync();
        _connectionLock.Dispose();
    }
}

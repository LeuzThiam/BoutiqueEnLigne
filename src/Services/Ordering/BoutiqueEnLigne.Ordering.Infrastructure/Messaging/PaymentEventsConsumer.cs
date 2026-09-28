using System.Text.Json;
using BoutiqueEnLigne.Contracts.Payments;
using BoutiqueEnLigne.EventBus;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;

namespace BoutiqueEnLigne.Ordering.Infrastructure.Messaging;

public sealed class PaymentEventsConsumer : BackgroundService
{
    private const string QueueName = "ordering.payments.v1";
    private const string DeadLetterExchangeName = "boutique.dead-letter";
    private const string DeadLetterQueueName = "ordering.payments.dead-letter.v1";
    private readonly RabbitMqOptions _options;
    private readonly IServiceScopeFactory _scopeFactory;
    private readonly ILogger<PaymentEventsConsumer> _logger;
    private IConnection? _connection;
    private IChannel? _channel;

    public PaymentEventsConsumer(
        RabbitMqOptions options,
        IServiceScopeFactory scopeFactory,
        ILogger<PaymentEventsConsumer> logger)
    {
        _options = options;
        _scopeFactory = scopeFactory;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                await ConsumeAsync(stoppingToken);
            }
            catch (OperationCanceledException) when (stoppingToken.IsCancellationRequested)
            {
                break;
            }
            catch (Exception exception)
            {
                _logger.LogWarning(exception, "Payment consumer disconnected; reconnecting.");
                await Task.Delay(TimeSpan.FromSeconds(5), stoppingToken);
            }
        }
    }

    private async Task ConsumeAsync(CancellationToken cancellationToken)
    {
        var factory = new ConnectionFactory
        {
            HostName = _options.Host,
            Port = _options.Port,
            UserName = _options.UserName,
            Password = _options.Password,
            VirtualHost = _options.VirtualHost,
            AutomaticRecoveryEnabled = true,
            TopologyRecoveryEnabled = true,
            ClientProvidedName = "ordering-payment-consumer"
        };

        _connection = await factory.CreateConnectionAsync(cancellationToken);
        _channel = await _connection.CreateChannelAsync(cancellationToken: cancellationToken);
        await _channel.ExchangeDeclareAsync(
            _options.ExchangeName, ExchangeType.Topic, durable: true, autoDelete: false,
            cancellationToken: cancellationToken);
        await _channel.ExchangeDeclareAsync(
            DeadLetterExchangeName, ExchangeType.Topic, durable: true, autoDelete: false,
            cancellationToken: cancellationToken);
        await _channel.QueueDeclareAsync(
            DeadLetterQueueName, durable: true, exclusive: false, autoDelete: false,
            cancellationToken: cancellationToken);
        await _channel.QueueBindAsync(
            DeadLetterQueueName, DeadLetterExchangeName, "#",
            cancellationToken: cancellationToken);
        await _channel.QueueDeclareAsync(
            QueueName, durable: true, exclusive: false, autoDelete: false,
            arguments: new Dictionary<string, object?>
            {
                ["x-dead-letter-exchange"] = DeadLetterExchangeName
            },
            cancellationToken: cancellationToken);
        await _channel.QueueBindAsync(
            QueueName, _options.ExchangeName, PaymentSucceededIntegrationEvent.EventName,
            cancellationToken: cancellationToken);
        await _channel.QueueBindAsync(
            QueueName, _options.ExchangeName, PaymentFailedIntegrationEvent.EventName,
            cancellationToken: cancellationToken);
        await _channel.BasicQosAsync(0, 1, global: false, cancellationToken);

        var consumer = new AsyncEventingBasicConsumer(_channel);
        consumer.ReceivedAsync += HandleMessageAsync;
        await _channel.BasicConsumeAsync(QueueName, autoAck: false, consumer, cancellationToken);

        _logger.LogInformation("Ordering is consuming payment events from {QueueName}", QueueName);
        await Task.Delay(Timeout.InfiniteTimeSpan, cancellationToken);
    }

    private async Task HandleMessageAsync(object sender, BasicDeliverEventArgs args)
    {
        if (_channel is null) return;

        var payload = args.Body.ToArray();
        var eventName = args.RoutingKey;
        try
        {
            using var scope = _scopeFactory.CreateScope();
            var handler = scope.ServiceProvider.GetRequiredService<PaymentEventHandler>();
            await handler.HandleAsync(eventName, payload, args.CancellationToken);
            await _channel.BasicAckAsync(args.DeliveryTag, multiple: false, args.CancellationToken);
        }
        catch (JsonException exception)
        {
            _logger.LogError(exception, "Discarding malformed integration event {EventName}", eventName);
            await _channel.BasicNackAsync(args.DeliveryTag, multiple: false, requeue: false, args.CancellationToken);
        }
        catch (Exception exception)
        {
            _logger.LogError(exception, "Payment event {EventName} moved to the dead-letter queue", eventName);
            await _channel.BasicNackAsync(args.DeliveryTag, multiple: false, requeue: false, args.CancellationToken);
        }
    }

    public override async Task StopAsync(CancellationToken cancellationToken)
    {
        if (_channel is not null) await _channel.DisposeAsync();
        if (_connection is not null) await _connection.DisposeAsync();
        await base.StopAsync(cancellationToken);
    }
}

using BoutiqueEnLigne.EventBus;
using BoutiqueEnLigne.Payments.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace BoutiqueEnLigne.Payments.Infrastructure.Messaging;

public sealed class PaymentsOutboxProcessor : BackgroundService
{
    private readonly IServiceScopeFactory _scopeFactory;
    private readonly IEventBus _eventBus;
    private readonly ILogger<PaymentsOutboxProcessor> _logger;

    public PaymentsOutboxProcessor(IServiceScopeFactory scopeFactory, IEventBus eventBus, ILogger<PaymentsOutboxProcessor> logger)
    {
        _scopeFactory = scopeFactory;
        _eventBus = eventBus;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            try { await ProcessBatchAsync(stoppingToken); }
            catch (OperationCanceledException) when (stoppingToken.IsCancellationRequested) { break; }
            catch (Exception exception)
            {
                _logger.LogWarning(exception, "Payments outbox processing failed; messages remain pending.");
            }
            await Task.Delay(TimeSpan.FromSeconds(5), stoppingToken);
        }
    }

    private async Task ProcessBatchAsync(CancellationToken cancellationToken)
    {
        using var scope = _scopeFactory.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<PaymentsDbContext>();
        var messages = await dbContext.OutboxMessages
            .Where(message => message.ProcessedAtUtc == null && message.RetryCount < 10)
            .OrderBy(message => message.OccurredAtUtc).Take(20).ToListAsync(cancellationToken);

        foreach (var message in messages)
        {
            try
            {
                await _eventBus.PublishAsync(message.Type, message.Id, message.Payload, cancellationToken);
                message.ProcessedAtUtc = DateTime.UtcNow;
                message.LastError = null;
            }
            catch (Exception exception)
            {
                message.RetryCount++;
                message.LastError = exception.Message[..Math.Min(exception.Message.Length, 2000)];
                _logger.LogWarning(exception, "Could not publish payment message {MessageId}", message.Id);
            }
            await dbContext.SaveChangesAsync(cancellationToken);
        }
    }
}

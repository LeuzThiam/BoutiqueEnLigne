using BoutiqueEnLigne.Ordering.Application.Abstractions;
using BoutiqueEnLigne.Ordering.Infrastructure.Persistence;
using BoutiqueEnLigne.Ordering.Infrastructure.Messaging;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace BoutiqueEnLigne.Ordering.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddOrderingInfrastructure(this IServiceCollection services, string connectionString)
    {
        services.AddDbContext<OrderingDbContext>(options => options.UseSqlServer(connectionString));
        services.AddScoped<IOrderRepository, OrderRepository>();
        services.AddScoped<PaymentEventHandler>();
        services.AddHostedService<OrderingOutboxProcessor>();
        services.AddHostedService<PaymentEventsConsumer>();
        return services;
    }
}

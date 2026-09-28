using Microsoft.Extensions.DependencyInjection;

namespace BoutiqueEnLigne.EventBus;

public static class DependencyInjection
{
    public static IServiceCollection AddRabbitMqEventBus(
        this IServiceCollection services,
        RabbitMqOptions options)
    {
        services.AddSingleton(options);
        services.AddSingleton<IEventBus, RabbitMqEventBus>();
        return services;
    }
}

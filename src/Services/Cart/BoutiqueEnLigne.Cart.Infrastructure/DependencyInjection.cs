using BoutiqueEnLigne.Cart.Application.Abstractions;
using BoutiqueEnLigne.Cart.Infrastructure.Persistence;
using Microsoft.Extensions.DependencyInjection;

namespace BoutiqueEnLigne.Cart.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddCartInfrastructure(this IServiceCollection services, string redisConnectionString)
    {
        services.AddStackExchangeRedisCache(options => options.Configuration = redisConnectionString);
        services.AddScoped<ICartRepository, RedisCartRepository>();
        return services;
    }
}

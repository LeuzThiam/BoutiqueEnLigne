using BoutiqueEnLigne.Payments.Application.Abstractions;
using BoutiqueEnLigne.Payments.Infrastructure.Persistence;
using BoutiqueEnLigne.Payments.Infrastructure.Stripe;
using BoutiqueEnLigne.Payments.Infrastructure.Messaging;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace BoutiqueEnLigne.Payments.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddPaymentsInfrastructure(
        this IServiceCollection services,
        string connectionString,
        StripeOptions stripeOptions)
    {
        services.AddDbContext<PaymentsDbContext>(options => options.UseSqlServer(connectionString));
        services.AddSingleton(stripeOptions);
        services.AddScoped<IPaymentService, StripePaymentService>();
        services.AddHostedService<PaymentsOutboxProcessor>();
        return services;
    }
}

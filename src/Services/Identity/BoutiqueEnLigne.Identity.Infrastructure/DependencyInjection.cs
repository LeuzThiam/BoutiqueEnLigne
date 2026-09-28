using BoutiqueEnLigne.Identity.Application.Abstractions;
using BoutiqueEnLigne.Identity.Domain.Entities;
using BoutiqueEnLigne.Identity.Infrastructure.Persistence;
using BoutiqueEnLigne.Identity.Infrastructure.Security;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace BoutiqueEnLigne.Identity.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddIdentityInfrastructure(
        this IServiceCollection services,
        string connectionString,
        JwtOptions jwtOptions)
    {
        services.AddDbContext<IdentityDbContext>(options => options.UseSqlServer(connectionString));
        services.AddScoped<IPasswordHasher<User>, PasswordHasher<User>>();
        services.AddSingleton(jwtOptions);
        services.AddSingleton<ITokenService, JwtTokenService>();
        services.AddScoped<IIdentityService, IdentityService>();
        return services;
    }
}

using System.Reflection;
using Xunit;

namespace BoutiqueEnLigne.Architecture.Tests;

public sealed class DomainDependencyTests
{
    private static readonly Assembly[] DomainAssemblies =
    [
        typeof(global::BoutiqueEnLigne.Identity.Domain.Entities.User).Assembly,
        typeof(global::BoutiqueEnLigne.Catalog.Domain.Entities.Product).Assembly,
        typeof(global::BoutiqueEnLigne.Cart.Domain.Entities.ShoppingCart).Assembly,
        typeof(global::BoutiqueEnLigne.Ordering.Domain.Entities.Order).Assembly,
        typeof(global::BoutiqueEnLigne.Payments.Domain.Entities.Payment).Assembly
    ];

    [Fact]
    public void Domain_projects_do_not_reference_framework_or_infrastructure_packages()
    {
        string[] forbiddenPrefixes =
        [
            "Microsoft.AspNetCore",
            "Microsoft.EntityFrameworkCore",
            "RabbitMQ",
            "Stripe",
            "StackExchange.Redis"
        ];

        foreach (var assembly in DomainAssemblies)
        {
            var forbiddenReferences = assembly.GetReferencedAssemblies()
                .Where(reference => forbiddenPrefixes.Any(prefix =>
                    reference.Name?.StartsWith(prefix, StringComparison.Ordinal) == true))
                .Select(reference => reference.Name)
                .ToArray();

            Assert.True(
                forbiddenReferences.Length == 0,
                $"{assembly.GetName().Name} has forbidden references: {string.Join(", ", forbiddenReferences)}");
        }
    }
}

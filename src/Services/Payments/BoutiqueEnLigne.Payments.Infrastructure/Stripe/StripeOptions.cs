namespace BoutiqueEnLigne.Payments.Infrastructure.Stripe;

public sealed class StripeOptions
{
    public string SecretKey { get; init; } = string.Empty;
    public string PublishableKey { get; init; } = string.Empty;
    public string WebhookSecret { get; init; } = string.Empty;

    public bool IsConfigured() =>
        IsRealValue(SecretKey) && IsRealValue(PublishableKey);

    private static bool IsRealValue(string value) =>
        !string.IsNullOrWhiteSpace(value) && !value.StartsWith("CHANGE_ME_", StringComparison.OrdinalIgnoreCase);
}

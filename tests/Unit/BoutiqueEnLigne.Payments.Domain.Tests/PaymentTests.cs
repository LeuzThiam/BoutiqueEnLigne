using BoutiqueEnLigne.Payments.Domain.Entities;
using BoutiqueEnLigne.Payments.Domain.Enums;
using Xunit;

namespace BoutiqueEnLigne.Payments.Domain.Tests;

public sealed class PaymentTests
{
    [Fact]
    public void MarkSucceeded_records_status_and_timestamp()
    {
        var payment = new Payment();
        var occurredAt = new DateTime(2026, 9, 28, 12, 0, 0, DateTimeKind.Utc);

        payment.MarkSucceeded(occurredAt);

        Assert.Equal(PaymentStatus.Succeeded, payment.Status);
        Assert.Equal(occurredAt, payment.UpdatedAtUtc);
    }

    [Fact]
    public void MarkFailed_records_status_and_timestamp()
    {
        var payment = new Payment();
        var occurredAt = new DateTime(2026, 9, 28, 12, 0, 0, DateTimeKind.Utc);

        payment.MarkFailed(occurredAt);

        Assert.Equal(PaymentStatus.Failed, payment.Status);
        Assert.Equal(occurredAt, payment.UpdatedAtUtc);
    }
}

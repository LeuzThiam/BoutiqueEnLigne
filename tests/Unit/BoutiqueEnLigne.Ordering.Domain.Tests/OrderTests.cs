using BoutiqueEnLigne.Ordering.Domain.Entities;
using BoutiqueEnLigne.Ordering.Domain.Enums;
using Xunit;

namespace BoutiqueEnLigne.Ordering.Domain.Tests;

public sealed class OrderTests
{
    [Fact]
    public void ConfirmPayment_marks_order_as_paid_and_confirmed()
    {
        var order = new Order();

        order.ConfirmPayment();

        Assert.True(order.EstPayee);
        Assert.Equal(OrderStatus.Confirmed, order.Statut);
    }

    [Fact]
    public void RegisterPaymentFailure_keeps_order_available_for_another_attempt()
    {
        var order = new Order { EstPayee = true, Statut = OrderStatus.Confirmed };

        order.RegisterPaymentFailure();

        Assert.False(order.EstPayee);
        Assert.Equal(OrderStatus.Pending, order.Statut);
    }
}

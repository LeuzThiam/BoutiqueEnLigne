using BoutiqueEnLigne.Ordering.Domain.Enums;

namespace BoutiqueEnLigne.Ordering.Domain.Entities
{
    public class Order
    {
        public int Id { get; set; }
        public DateTime DateCommande { get; set; } = DateTime.UtcNow;
        public decimal Total { get; set; }
        public bool EstPayee { get; set; }
        public int UtilisateurId { get; set; }
        public OrderStatus Statut { get; set; } = OrderStatus.Pending;
        public List<OrderItem> ArticlesCommandes { get; set; } = [];

        public void ConfirmPayment()
        {
            EstPayee = true;
            Statut = OrderStatus.Confirmed;
        }

        public void RegisterPaymentFailure()
        {
            EstPayee = false;
            Statut = OrderStatus.Pending;
        }
    }
}

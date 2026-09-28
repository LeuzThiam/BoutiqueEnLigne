namespace BoutiqueEnLigne.Identity.Domain.Entities
{
    public class Seller
    {
        public int Id { get; set; }
        public string Nom { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public bool Actif { get; set; }
    }
}

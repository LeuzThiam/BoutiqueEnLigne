namespace BoutiqueEnLigne.Identity.Domain.Entities;

public sealed class RefreshToken
{
    public long Id { get; set; }
    public int UserId { get; set; }
    public string TokenHash { get; set; } = string.Empty;
    public DateTime ExpiresAtUtc { get; set; }
    public DateTime CreatedAtUtc { get; set; }
    public DateTime? RevokedAtUtc { get; set; }
    public User User { get; set; } = null!;

    public bool IsActive(DateTime utcNow) => RevokedAtUtc is null && ExpiresAtUtc > utcNow;
}

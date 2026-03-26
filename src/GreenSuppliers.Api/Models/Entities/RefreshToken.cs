using System.ComponentModel.DataAnnotations;

namespace GreenSuppliers.Api.Models.Entities;

public class RefreshToken
{
    public Guid Id { get; set; }

    public Guid UserId { get; set; }

    [MaxLength(500)]
    public string TokenHash { get; set; } = string.Empty;

    public DateTime ExpiresAt { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? RevokedAt { get; set; }
    public Guid? ReplacedByTokenId { get; set; }

    // Navigation
    public User User { get; set; } = null!;
}

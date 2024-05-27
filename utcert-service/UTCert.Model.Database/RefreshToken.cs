using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace UTCert.Model.Database;

public class RefreshToken
{
    [Key]
    public Guid Id { get; set; }

    [ForeignKey(nameof(User))]
    public Guid UserId { get; set; }
    
    public User User { get; set; }
    public string Token { get; set; }
    public DateTime Expires { get; set; }
    public DateTime Created { get; set; }
    public string CreatedByIp { get; set; }
    public DateTime? Revoked { get; set; }
    public string? RevokedByIp { get; set; }
    public string? ReplacedByToken { get; set; }
    public string? ReasonRevoked { get; set; }
    [NotMapped]
    public bool IsExpired => DateTime.UtcNow >= Expires;
    [NotMapped]
    public bool IsRevoked => Revoked != null;
    [NotMapped]
    public bool IsActive => Revoked == null && !IsExpired;
}
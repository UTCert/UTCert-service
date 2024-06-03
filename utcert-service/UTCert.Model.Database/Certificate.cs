using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using UTCert.Model.Shared.Enum;

namespace UTCert.Model.Database;

public class Certificate
{
    [Key]
    public Guid Id { get; set; }
    public long Code { get; set; }
    public Guid IssuerId { get; set; }
    public Guid ReceiverId { get; set; }
    public string? Name { get; set; }
    public string? IpfsLink { get; set; }
    public string? ImageLink { get; set; }
    public CertificateStatus Status { get; set; }
    
    public string? ReceiverAddressWallet { get; set; }
    public string? ReceiverIdentityNumber { get; set; }
    public string? ReceiverName { get; set; }
    public DateTime? ReceiverDoB { get; set; }
    
    public int GraduationYear { get; set; }
    public string? Classification { get; set; }
    public StudyMode StudyMode { get; set; }
    
    public DateTime CreatedDate { get; set; }
    public DateTime? ModifiedDate { get; set; }
    public DateTime? SignedDate { get; set; }
    public SigningType? SigningType { get; set; }
    public DateTime? SentDate { get; set; }
    public bool IsBanned { get; set; }
    public bool IsDeleted { get; set; }

    [ForeignKey("IssuerId")]
    public virtual User? Issuer { get; set; }

    [ForeignKey("ReceiverId")]
    public virtual User? Receiver { get; set; }
}
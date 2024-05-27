using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using UTCert.Model.Database;

namespace UTCert.Models.Database;

public class Certificate
{
    [Key]
    public Guid Id { get; set; }

    public Guid IssuerId { get; set; }
    public Guid ReceiverId { get; set; }
    public string? Name { get; set; }
    public string? IpfsLink { get; set; }
    public short Status { get; set; }
    
    public string? ReceiverAddressWallet { get; set; }
    public string? ReceiverIdentityNumber { get; set; }
    public string? ReceiverName { get; set; }
    public DateTime? ReceiverDoB { get; set; }
    
    public int GraduationYear { get; set; }
    public string? Classification { get; set; }
    public short StudyMode { get; set; }
    
    public DateTime CreatedDate { get; set; }
    public DateTime? SignedDate { get; set; }
    public short SigningType { get; set; }
    public DateTime? SentDate { get; set; }
    public bool IsBanned { get; set; }
    public bool IsDeleted { get; set; }

    [ForeignKey("IssuerId")]
    public virtual User? Issuer { get; set; }

    [ForeignKey("ReceiverId")]
    public virtual User? Receiver { get; set; }
}
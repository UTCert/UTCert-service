using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using UTCert.Model.Shared.Enum;

namespace UTCert.Model.Database;

public class Contact
{
    [Key]
    public Guid Id { get; set; }
    public Guid IssuerId { get; set; }
    public Guid ReceiverId { get; set; }
    public ContactStatus Status { get; set; }
    public DateTime CreatedDate { get; set; }
    public DateTime? ModifiedDate { get; set; }

    [ForeignKey("IssuerId")]
    public virtual User Issuer { get; set; }

    [ForeignKey("ReceiverId")]
    public virtual User Receiver { get; set; }

    public string IssuerName { get; set; }
    public string ReceiverName { get; set; }
}
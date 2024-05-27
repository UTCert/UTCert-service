using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using UTCert.Model.Database;

namespace UTCert.Models.Database;

public class Contact
{
    [Key]
    public Guid Id { get; set; }

    public Guid IssuerId { get; set; }
    public Guid ReceiverId { get; set; }
    public short Status { get; set; }
    public DateTime CreatedDate { get; set; }

    [ForeignKey("IssuerId")]
    public virtual User? Issuer { get; set; }

    [ForeignKey("ReceiverId")]
    public virtual User? Receiver { get; set; }
}
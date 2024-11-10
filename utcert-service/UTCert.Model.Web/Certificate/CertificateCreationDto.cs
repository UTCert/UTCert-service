using UTCert.Model.Shared.Enum;

namespace UTCert.Model.Web.Certificate;

public class CertificateCreationDto
{
    public string StakeId { get; set; }
    public string AddressWallet { get; set; }
    public string IdentityNumber { get; set; }
    public string CertificateName { get; set; }
    public string ReceiverName { get; set; }
    public DateTime DateOfBirth { get; set; }
    public int GraduationYear { get; set; }
    public string Classification { get; set; }
    public StudyMode StudyMode { get; set; }
    public SigningType SigningType { get; set; }
    public string? SignerAddress { get; set; }
    public string? Attachment { get; set; }
}
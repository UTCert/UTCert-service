using UTCert.Model.Web.Dtos;

namespace UTCert.Service.BusinessLogic.Dtos
{
    public class CertificateFilterInputDto : PagedInputDto
    {
        public Guid? IssuerId { get; set; }
        public string? IssuerAddress { get; set; }
        public Guid? ReceiverId { get; set; }
        public string? ReceiverAddress { get; set; }
        public int? CertificateStatus { get; set; }
        public string? CertificateName { get; set; }
        public string? ReceivedName { get; set; }
        public string? OrganizationName { get; set; }
    }

}

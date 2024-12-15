using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UTCert.Model.Shared.Enum;

namespace UTCert.Model.Web.Certificate
{
    public class CertificateUploadDto
    {
        public Guid Id { get; set; }
        public string? Note { get; set; }
        public string? Attachment { get; set; }
        public string? AttachmentName { get; set; }
        public CertificateStatus? Status { get; set; }
    }
}

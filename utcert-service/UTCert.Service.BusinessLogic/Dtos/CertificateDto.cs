using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using UTCert.Model.Database;
using UTCert.Model.Shared.Enum;

namespace UTCert.Service.BusinessLogic.Dtos
{
    public class CertificateDto : Certificate
    {
        public int? ContactStatus { get; set; }
        public Guid? ContactId { get; set; }
    }

    // JSON thông tin khi multiple signature
    public class CertificateMulSignDto
    {

        public string IssuerName { get; set; }  
        public string IssuerAddress { get; set; }
        public DateTime? SignedDate { get; set; }
        public bool IsSigned { get; set; }
    }

}

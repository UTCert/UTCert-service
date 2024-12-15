using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UTCert.Model.Shared.Enum;

namespace UTCert.Model.Web.Certificate
{
    public class SignCertificateRequest 
    { 
        public Guid CertificateId { get; set; }
        public SigningType SigningType { get; set; }
        public string SignHash { get; set; }    
        public string IssuerAddress { get; set; }
    }
}

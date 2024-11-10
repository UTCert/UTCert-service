using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UTCert.Model.Shared.Enum;

namespace UTCert.Model.Web.Contact
{
    public class ContactInputDto
    {
        public Guid Id { get; set; }
        public ContactStatus Status { get; set; }
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UTCert.Model.Web.Dtos;

namespace UTCert.Model.Web.Contact
{
    public class ContactFilterDto : PagedInputDto
    {
        public Guid? UserId { get; set; }
        public int? ContactStatus { get; set; }
        public string? ContactName { get; set; }
    }
}

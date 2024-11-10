using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UTCert.Model.Database;
using UTCert.Model.Shared.Enum;

namespace UTCert.Service.BusinessLogic.Dtos
{
    public class ContactDto : Contact
    {
        public string ContactName { get; set; }

        public int ContactStatus
        {
            get { return (int)Status; }
        }
    }
}

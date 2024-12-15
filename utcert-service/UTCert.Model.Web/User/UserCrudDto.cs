using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace UTCert.Model.Web.User
{
    public class UserCrudDto 
    {
        public string Name { get; set; }
        public IFormFile? AvatarUri { get; set; }
    }
}

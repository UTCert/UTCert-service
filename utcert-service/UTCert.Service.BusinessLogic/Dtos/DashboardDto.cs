using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace UTCert.Service.BusinessLogic.Dtos
{
    public class DashboardDto
    {
        public string Username { get; set; }// tên tài khoản
        public bool IsVerified { get; set; } // tài khoản được Verify hay chưa
        public string AvatarUri { get; set; }    // url Logo
        public int Pending { get; set; }    // số contact đang pending
        public int Accepted { get; set; }  // số contact đang accepted
        public int Draft { get; set; }      // số certificate đang draft
        public int Signed { get; set; }     // số certificate đã kí
        public int Sent { get; set; }       // số certificate đã gửi
        public int Banned { get; set; }     // số certificate đã ban
        public int Received { get; set; }   // số certificate nhận được
    }
}

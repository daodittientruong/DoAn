
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Payloads.RequestModels.UserRequests
{
    public class Request_ChangePassword
    {
        public string UserName { get; set; }
        public string OldPassword { get; set; } 
        public string NewPassword { get; set; }
        public string ConfirmPassword { get; set; }
    }
}

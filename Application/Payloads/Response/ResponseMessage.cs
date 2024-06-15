using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Payloads.Response
{
    public static class ResponseMessage
    {
        public static string GetEmailSuccessMessage(string email)
        {
            return $"Email đã được gửi đến: {email}";
        }
    }
}

using Application.Payloads.ResponseModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security;
using System.Text;
using System.Threading.Tasks;

namespace DoAn.Application.Payloads.ResponseModels.DataUsers
{
    public class DataResponseUser : DataResponseBase
    {
        public string Email { get; set; }
        public string PhoneNumeber { get; set; }
        public DateTime DateOfBirth { get; set; }
        public DateTime CreateTime { get; set; }
        public DateTime? UpdateTime { get; set; }
        public string FullName { get; set; }
        public string UserStatus { get; set; } 
    }
}

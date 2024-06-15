using DoAn.Domain.Entities;
using DoAn.Domain.Enumartes;
using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Payloads.RequestModels.UserRequests
{
    public class Request_UpdateUser
    { 
        public string Email { get; set; }
        public string PhoneNumeber { get; set; }
        public DateTime DateOfBirth { get; set; }
        
        public IFormFile FullName { get; set; }
        
    }
}

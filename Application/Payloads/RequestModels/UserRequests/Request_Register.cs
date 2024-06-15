using DoAn.Domain.Entities;
using DoAn.Domain.Enumartes;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Payloads.RequestModels.UserRequests
{
    public class Request_Register
    {
        [Required(ErrorMessage = "Tên đăng nhập không được bỏ trống. Tên đăng nhập là mã sinh viên")] 
        public string UserName { get; set; }
        [Required(ErrorMessage = "Mật khẩu không được bỏ trống")]
        public string Password { get; set; }
        [Required(ErrorMessage = "Họ và tên không được bỏ trống")]
        public string FullName { get; set; }
        [Required(ErrorMessage = "Email không được bỏ trống")]
        public string Email { get; set; }
        [Required(ErrorMessage = "Ngày sinh không được bỏ trống")]
        public DateTime DateOfBirth { get; set; }
        [Required(ErrorMessage = "Số điện thoại không được bỏ trống")]
        public string PhoneNumber { get; set; }
    }
}

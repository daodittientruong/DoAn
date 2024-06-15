using DoAn.Domain.Enumartes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DoAn.Domain.Entities
{
    public class User : Base
    {
        public string UserName { get; set; }
        public string Password { get; set; }
        public string Email { get; set; }
        public string PhoneNumeber { get; set; }
        public DateTime DateOfBirth { get; set; }
        public DateTime CreateTime { get; set; }    
        public DateTime? UpdateTime { get; set; }
        public string FullName { get; set; }
        public virtual ICollection<Permission>? Users { get; set; }
        public ConstantEnums.UserStatusEnum UserStatus { get; set; } = ConstantEnums.UserStatusEnum.UnActivated;
    }
}

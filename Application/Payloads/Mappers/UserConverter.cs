using DoAn.Application.Payloads.ResponseModels.DataUsers;
using DoAn.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Payloads.Mappers
{
    public class UserConverter
    {
        public DataResponseUser EntityToDTO(User user)
        {
            return new DataResponseUser
            {
                CreateTime = user.CreateTime,
                DateOfBirth = user.DateOfBirth,
                Email = user.Email,
                FullName = user.FullName,
                Id = user.Id,
                PhoneNumeber = user.PhoneNumeber,
                UpdateTime = user.UpdateTime,
                UserStatus = user.UserStatus.ToString(),
            };
        }
    }
}

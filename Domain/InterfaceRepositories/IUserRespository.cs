using DoAn.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DoAn.Domain.InterfaceRepositories
{
    public interface IUserRespository
    {
        Task<User> GetUserByEmail(string email);
        Task<User> GetUserByUsername(string username);
        //Task<User> GetUserByPhoneNumber(string phoneNumber);
        Task AddRoleToUserAsync(User user, List<string> listRole);
        Task<IEnumerable<string>> GetRoleOfUserAsync(User user);
        Task DeleteRolesAsync(User user, List<string> roles);
    }
}

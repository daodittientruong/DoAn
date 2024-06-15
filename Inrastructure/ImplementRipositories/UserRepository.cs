using DoAn.Domain.Entities;
using DoAn.Domain.InterfaceRepositories;
using DoAn.Infrastructure.DataContext;
using Domain.InterfaceRepositories;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DoAn.Infrastructure.ImplementRipositories
{
    public class UserRepository : IUserRespository
    {
        private readonly ApplicationDbContext _context;
        public UserRepository(ApplicationDbContext context)
        {
            _context = context;
        }
        #region Xử lí chuỗi
        private Task<bool> CompareStringAsync(string str1, string str2)
        {
            return Task.FromResult(string.Equals(str1.ToLowerInvariant(), str2.ToLowerInvariant()));
        }
        private async Task<bool> IsStringInListAsync(string inputString, List<string> listString)
        {
            if (inputString == null)
            {
                throw new ArgumentNullException(nameof(inputString));
            }
            if (listString == null)
            {
                throw new ArgumentNullException(nameof(inputString));
            }
            foreach (var item in listString)
            {
                if(await CompareStringAsync(inputString, item))
                {
                    return true;
                }
            }
            return false;
        }
        #endregion
        public async Task AddRoleToUserAsync(User user, List<string> listRole)
        {
            if (user == null)
            {
                throw new ArgumentNullException(nameof(user));
            }
            if (listRole == null)
            {
                throw new ArgumentNullException(nameof(listRole));
            }
            foreach (var role in listRole.Distinct())
            {
                var roleofUser = await GetRoleOfUserAsync(user);
                if (await IsStringInListAsync(role, roleofUser.ToList()))
                {
                    throw new ArgumentException("Người dùng đã có quyền này rồi");
                }
                else
                {
                    var roleItem = await _context.Roles.SingleOrDefaultAsync(x => x.RoleCode.Equals(role));
                    if (roleItem == null)
                    {
                        throw new ArgumentException("Không tồn tại quyền này");
                    }
                    _context.Permissions.Add(new Permission
                    {
                        RoleId = roleItem.Id,
                        UserId = user.Id,
                    });
                } 
            }
            _context.SaveChanges();
        }

        public async Task<IEnumerable<string>> GetRoleOfUserAsync(User user)
        {
            var roles = new List<string>();
            var listRoles = _context.Permissions.Where(x => x.UserId == user.Id).AsQueryable();
            foreach (var item in listRoles.Distinct())
            {
                var role = _context.Roles.SingleOrDefault(x => x.Id == item.RoleId);
                roles.Add(role.RoleCode);
            }
            return roles.AsEnumerable();
        }

        public async  Task<User> GetUserByEmail(string email)
        {
           var user = await _context.Users.SingleOrDefaultAsync(x => x.Email.ToLower().Equals(email.ToLower()));
           return user;    
        }

        public async Task<User> GetUserByPhoneNumber(string phoneNumber)
        {
            var user = await _context.Users.SingleOrDefaultAsync(x => x.PhoneNumeber.ToLower().Equals(phoneNumber.ToLower()));
            return user;
        }

        public async Task<User> GetUserByUsername(string username)
        {
            var user = await _context.Users.SingleOrDefaultAsync(x => x.UserName.ToLower().Equals(username.ToLower()));
            return user;
        }

        public async Task DeleteRolesAsync(User user, List<string> roles)
        {
            var listRole = await GetRoleOfUserAsync(user);
            if(roles == null)
            {
                throw new ArgumentNullException(nameof(roles));
            }
            if(listRole == null)
            {
                throw new ArgumentNullException(nameof(listRole));
            }
            foreach (var role in listRole) 
            {
                foreach(var roleItem in roles)
                {
                    var roleObject = _context.Roles.SingleOrDefault(x => x.RoleCode.Equals(roleItem));
                    var permission = _context.Permissions.SingleOrDefault(x => x.RoleId == roleObject.Id && x.UserId == user.Id);
                    if(await CompareStringAsync(role, roleItem))
                    {
                        _context.Permissions.Remove(permission);
                    }
                }
            }
            _context.SaveChanges();
        }
    }
}

using Application.Handle.HandleEmail.HandleFile;
using Application.InterfaceService;
using Application.Payloads.Mappers;
using Application.Payloads.RequestModels.UserRequests;
using DoAn.Application.Payloads.Response;
using DoAn.Application.Payloads.ResponseModels.DataUsers;
using DoAn.Domain.Entities;
using Domain.InterfaceRepositories;
using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.ImplementService
{
    public class UserService : IUserService
    {
        private readonly IBaseRepository<User> _repository;
        private readonly UserConverter _converter;
        private readonly IHttpContextAccessor _httpContextAccessor;
        public UserService(IBaseRepository<User> repository, UserConverter converter, IHttpContextAccessor httpContextAccessor)
        {
            _repository = repository;
            _converter = converter;
            _httpContextAccessor = httpContextAccessor;
        }

        public async Task<ResponseObject<DataResponseUser>> UpdateUser(long userId, Request_UpdateUser request)
        {
            var currentUser = _httpContextAccessor.HttpContext.User;
            try
            {
                if (!currentUser.Identity.IsAuthenticated) //Tài khoản chưa xác thực
                {
                    return new ResponseObject<DataResponseUser>
                    {
                        Status = StatusCodes.Status401Unauthorized,
                        Message = "Người dùng chưa được xác thực",
                        Data = null,
                    };
                }
                var user = currentUser.FindFirst("Id").Value;
                var userItem = await _repository.GetByIdAsync(userId);
                if (long.Parse(user) != userId && long.Parse(user) != userItem.Id)
                {
                    return new ResponseObject<DataResponseUser>
                    {
                        Status = StatusCodes.Status403Forbidden,
                        Message = "Bạn không có quyền thực hiện chức năng",
                        Data = null
                    };
                }
                
                userItem.FullName = await HandleUploadFile.WriteFile(request.FullName);
                userItem.PhoneNumeber = request.PhoneNumeber;
                userItem.DateOfBirth = request.DateOfBirth;
                userItem.Email = request.Email;
                userItem.UpdateTime = DateTime.Now;
                await _repository.UpdateAsync(userItem);
                return new ResponseObject<DataResponseUser>
                {
                    Status = StatusCodes.Status200OK,
                    Message = "Cập nhật thông tin người dùng thành công",
                    Data = _converter.EntityToDTO(userItem)
                };
            }
            catch (Exception ex)
            {
                throw;
            }
        }
        
    }
}

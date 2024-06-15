using Application.Handle.HandleEmail;
using Application.InterfaceService;
using Application.Payloads.Mappers;
using Application.Payloads.RequestModels.UserRequests;
using Application.Payloads.ResponseModels.DataUsers;
using AutoMapper;
using DoAn.Application.InterfaceService;
using DoAn.Application.Payloads.Response;
using DoAn.Application.Payloads.ResponseModels.DataUsers;
using DoAn.Domain.Entities;
using DoAn.Domain.InterfaceRepositories;
using DoAn.Domain.Validations;
using Domain.InterfaceRepositories;
using MailKit;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using BCryptNet = BCrypt.Net.BCrypt;

namespace Application.ImplementService
{
    public class AuthService : IAuthService
    {
        private readonly IBaseRepository<User> _baseUserRepository;
        private readonly UserConverter _userConverter;
        private readonly IConfiguration _configuration;
        private readonly IUserRespository _userRespository;
        private readonly IEmailService _emailService;
        private readonly IBaseRepository<ConfirmEmail> _baseConfirmEmaiRepository;
        private readonly IBaseRepository<Permission> _basePermissionRepository;
        private readonly IBaseRepository<Role> _baseRoleRepository;
        private readonly IBaseRepository<RefreshToken> _baseRefreshTokenRepository;
        private readonly IHttpContextAccessor _contextAccessor;

        public AuthService(IBaseRepository<User> baseUserRepository, UserConverter userConverter, IConfiguration configuration, IUserRespository userRespository, IEmailService emailService, IBaseRepository<ConfirmEmail> baseConfirmEmaiRepository, 
            IBaseRepository<Permission> basePermissionRepository, IBaseRepository<Role> baseRoleRepository, IBaseRepository<RefreshToken> baseRefreshTokenRepository, IHttpContextAccessor contextAccessor)
        {
            _baseUserRepository = baseUserRepository;
            _userConverter = userConverter;
            _configuration = configuration;
            _userRespository = userRespository;
            _emailService = emailService;
            _baseConfirmEmaiRepository = baseConfirmEmaiRepository;
            _basePermissionRepository = basePermissionRepository; 
            _baseRoleRepository = baseRoleRepository;
            _baseRefreshTokenRepository = baseRefreshTokenRepository;
            _contextAccessor = contextAccessor;
        }

        public async Task<string> ConfirmRegisterAccount(string confirmCode)
        {
            try
            {
                var code = await _baseConfirmEmaiRepository.GetAsync(x => x.ConfirmCode.Equals(confirmCode));
                if(code == null)
                {
                    return "Mã xác nhận không hợp lệ";
                }
                var user = await _baseUserRepository.GetAsync(x => x.Id == code.UserId);
                if (code.ExpiryTime < DateTime.Now)
                {
                    return "Mã xác nhận đã hết hạn";
                }
                user.UserStatus = DoAn.Domain.Enumartes.ConstantEnums.UserStatusEnum.Activated;
                code.IsConfirmed = true;
                await _baseUserRepository.UpdateAsync(user);
                await _baseConfirmEmaiRepository.UpdateAsync(code);
                return "Xác nhận đăng ký tài khoản thành công! Bạn có thể dùng tài khoản này để đăng nhập";
            }
            catch(Exception ex)
            {
                return ex.Message;
            }
        }

        public async Task<ResponseObject<DataResponseLogin>> GetJwtTokenAsync(User user)
        {
            var permissions = await _basePermissionRepository.GetAllAsync(x => x.UserId == user.Id);
            var role = await _baseRoleRepository.GetAllAsync();

            var authClaims = new List<Claim>
            {
                new Claim("Id", user.Id.ToString()),
                new Claim("Username", user.UserName.ToString()),
                new Claim("Email", user.Email.ToString()),
            };
            foreach(var permission in permissions)
            {
                foreach(var roles in role)
                {
                    if(roles.Id == permission.RoleId)
                    {
                        authClaims.Add(new Claim("Permission", roles.RoleName));
                    }
                }
            }
            var userRoles = await _userRespository.GetRoleOfUserAsync(user);
            foreach(var item in userRoles)
            {
                authClaims.Add(new Claim(ClaimTypes.Role, item));
            }
            var jwtToken = GetToken(authClaims);
            var refreshToken = GenerateRefreshToken();
            _ = int.TryParse(_configuration["JWT: RefreshTokenValidity"], out int refreshTokenValidity);
            RefreshToken rf = new RefreshToken
            {
                IsActive = true,
                ExpiryTime = DateTime.Now.AddHours(refreshTokenValidity),
                UserId = user.Id,
                Token = refreshToken
            };
            rf = await _baseRefreshTokenRepository.CreateAsync(rf);
            return new ResponseObject<DataResponseLogin>
            {
                Status = StatusCodes.Status200OK,
                Message = "Tạo token thành công",
                Data = new DataResponseLogin
                {
                    AccessToken = new JwtSecurityTokenHandler().WriteToken(jwtToken),
                    RefreshToken = refreshToken,
                }
            };
        }
        
        public async Task<ResponseObject<DataResponseLogin>> Login(Request_Login request)
        {
            var user = await _baseUserRepository.GetAsync(x => x.UserName.Equals(request.UserName));
            if(user == null)
            {
                return new ResponseObject<DataResponseLogin>
                {
                    Status = StatusCodes.Status400BadRequest,
                    Message = "Sai tên tài khoản",
                    Data = null
                };
            }
            bool checkPass = BCryptNet.Verify(request.Password, user.Password);
            if (!checkPass)
            {
                return new ResponseObject<DataResponseLogin>
                {
                    Status = StatusCodes.Status400BadRequest,
                    Message = "Mật khẩu không chính xác",
                    Data = null
                };
            }
            return new ResponseObject<DataResponseLogin>
            {
                Status = StatusCodes.Status200OK,
                Message = "Đăng nhập thành công",
                Data = new DataResponseLogin
                {
                    AccessToken = GetJwtTokenAsync(user).Result.Data.AccessToken,
                    RefreshToken = GetJwtTokenAsync(user).Result.Data.RefreshToken
                }
            };
        }

        public async Task<ResponseObject<DataResponseUser>> ChangePassword(long userId, Request_ChangePassword request)
        {
            try
            {
                var user = await _baseUserRepository.GetByIdAsync(userId); //Không cần check thông tin
                bool checkPass = BCryptNet.Verify(request.OldPassword, user.Password);
                if (!checkPass)
                {
                    return new ResponseObject<DataResponseUser>
                    {
                        Status = StatusCodes.Status400BadRequest,
                        Message = "Mật khẩu không chính xác",
                        Data = null
                    };
                }
                if (request.NewPassword.Equals(request.ConfirmPassword))
                {
                    return new ResponseObject<DataResponseUser>
                    {
                        Status = StatusCodes.Status400BadRequest,
                        Message = "Mật khẩu không trùng khớp",
                        Data = null
                    };
                }
                user.Password = BCryptNet.HashPassword(request.NewPassword);
                user.UpdateTime = DateTime.Now;
                await _baseUserRepository.UpdateAsync(user);
                return new ResponseObject<DataResponseUser>
                {
                    Status = StatusCodes.Status200OK,
                    Message = "Đổi mật khẩu thành công",
                    Data = _userConverter.EntityToDTO(user)
                };
            }
            catch (Exception ex)
            {
                return new ResponseObject<DataResponseUser>
                {
                    Status = StatusCodes.Status500InternalServerError,
                    Message = ex.Message,
                    Data = null
                };   
            }
        }
        public async Task<string> ForgotPassword(string email)
        {
            try
            {
                var user = await _userRespository.GetUserByEmail(email);
                if(user == null) 
                {
                    return "Email không tồn tại trong hệ thống";
                }
                var listConfirmCodes = await _baseConfirmEmaiRepository.GetAllAsync(x => x.UserId == user.Id);
                if(listConfirmCodes.ToList().Count > 0)
                {
                    foreach(var confirmCode in listConfirmCodes)
                    {
                        await _baseConfirmEmaiRepository.DeleteAsync(confirmCode.Id);
                    }
                }
                ConfirmEmail confirmEmail = new ConfirmEmail
                {
                    IsActive = true,
                    ConfirmCode = GenerateCodeActive(),
                    ExpiryTime = DateTime.Now.AddMinutes(1),
                    UserId = user.Id,
                    IsConfirmed = false
                };
                confirmEmail = await _baseConfirmEmaiRepository.CreateAsync(confirmEmail);
                var message = new EmailMessage(new string[] { user.Email }, "Nhận mã xác nhận tại đây: ", $"Mã xác nhận: {confirmEmail.ConfirmCode}");
                var send = _emailService.SendEmail(message);

                return "Gửi mã xác nhận về email thành công! Vui lòng kiểm tra email";
            }
            catch (Exception ex)
            {
                return ex.Message;
            }
        }

        public async Task<string> ConfirmCreateNewPassword(Request_CreateNewPassword request)
        {
            try
            {
                var confirmEmail = await _baseConfirmEmaiRepository.GetAsync(x => x.ConfirmCode.Equals(request.ConfirmCode));
                if(confirmEmail == null)
                {
                    return "Mã xác nhận không hợp lệ";
                }
                if(confirmEmail.ExpiryTime < DateTime.Now)
                {
                    return "Mã xác nhận đã hết hạn";
                }
                if (!request.NewPassword.Equals(request.ConfirmPassword))
                {
                    return "Mật khẩu không trùng khớp";
                }
                var user = await _baseUserRepository.GetAsync(x => x.Id == confirmEmail.UserId);
                user.Password = BCryptNet.HashPassword(request.NewPassword);
                user.UpdateTime = DateTime.Now;
                await _baseUserRepository.UpdateAsync(user);
                return "Tạo mật khẩu mới thành công"; 
            }
            catch(Exception ex)
            {
                return ex.Message;
            }
        }
        public async Task<string> AddRolesToUser(long userId, List<string> roles)
        {
            var currentUser = _contextAccessor.HttpContext.User;
            try
            {
                if (!currentUser.Identity.IsAuthenticated)
                {
                    return "Người dùng chưa được xác thực";
                }
                if (!currentUser.IsInRole("Admin"))
                {
                    return "Bạn không có quyền thực hiện chức năng này";
                }
                var user = await _baseUserRepository.GetByIdAsync(userId);
                if (user == null) 
                {
                    return "Không tìm thấy người dùng";
                }
                await _userRespository.AddRoleToUserAsync(user, roles);
                return "Thêm quyền cho người dùng thành công";
            }
            catch(Exception ex)
            {
                return ex.Message;
            }
        }

        public async Task<string> DeleteRoles(long userId, List<string> roles)
        {
            var currentUser = _contextAccessor.HttpContext.User;
            try
            {
                if (!currentUser.Identity.IsAuthenticated)
                {
                    return "Người dùng chưa được xác thực";
                }
                if (!currentUser.IsInRole("Admin"))
                {
                    return "Bạn không có quyền thực hiện chức năng này";
                }
                var user = await _baseUserRepository.GetByIdAsync(userId);
                if(user == null)
                {
                        return "Người dùng không tồn tại";
                }
                await _userRespository.DeleteRolesAsync(user, roles);
                return "Xóa quyền thành công";
            }
            catch (Exception ex)
            {
                return ex.Message;
            }
        }
        #region Private Method
        private JwtSecurityToken GetToken(List<Claim> authClaim)
        {
            var authSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_configuration["JWT:SecretKey"]));
            _ = int.TryParse(_configuration["JWT:TokenValidityInHours"], out int tokenValidityInHours);
            var exirationUTC = DateTime.Now.AddHours(tokenValidityInHours);


            var token = new JwtSecurityToken(
                issuer: _configuration["JWT:ValidIssuer"],
                audience: _configuration["JWT:ValidAudience"],
                expires: exirationUTC,
                claims: authClaim,
                signingCredentials: new SigningCredentials(authSigningKey, SecurityAlgorithms.HmacSha256)
                );
            return token;
        }
        private string GenerateRefreshToken()
        {
            var randomNumber = new Byte[64];
            var range = RandomNumberGenerator.Create();
            range.GetBytes(randomNumber);
            return Convert.ToBase64String(randomNumber);
        }
        #endregion
        public async Task<ResponseObject<DataResponseUser>> Register(Request_Register request)
        {
            try
            {
                if (!ValidateInput.IsValidEmail(request.Email))
                {
                    //Email sai định dạng vì !ValidateInput.IsValidEmail(request.Email) tương ứng với false
                    return new ResponseObject<DataResponseUser>
                    {
                        Status = StatusCodes.Status400BadRequest,
                        Message = "Định dạng email không hợp lệ",
                        Data = null
                    };
                }
                if (!ValidateInput.IsValidPhoneNumber(request.PhoneNumber))
                {
                    //Email sai định dạng vì !ValidateInput.IsValidEmail(request.Email)  tương ứng với false
                    return new ResponseObject<DataResponseUser>
                    {
                       Status = StatusCodes.Status400BadRequest,
                       Message = "Định dạng số điện thoại không hợp lệ",
                       Data = null
                    };
                }
                if (await _userRespository.GetUserByEmail(request.Email) != null)
                {
                    return new ResponseObject<DataResponseUser>
                    {
                        Status = StatusCodes.Status400BadRequest,
                        Message = "Email đã tồn tại trong hệ thống! Vui lòng sử dụng Email khác",
                        Data = null
                    };
                }
                if (await _userRespository.GetUserByUsername(request.UserName) != null)
                {
                    return new ResponseObject<DataResponseUser>
                    {
                        Status = StatusCodes.Status400BadRequest,
                        Message = "Mã sinh viên đã tồn tại trong hệ thống! ",
                        Data = null
                    };
                }
                var user = new User
                {
                    IsActive = true,
                    CreateTime = DateTime.Now,
                    DateOfBirth = request.DateOfBirth,
                    Email = request.Email,
                    Password = BCryptNet.HashPassword(request.Password),
                    PhoneNumeber = request.PhoneNumber,
                    UserName = request.UserName,
                    UserStatus = DoAn.Domain.Enumartes.ConstantEnums.UserStatusEnum.UnActivated,
                    FullName = "",
                };
                user = await _baseUserRepository.CreateAsync(user);
                await _userRespository.AddRoleToUserAsync(user, new List<string> { "Admin", "User" });
                ConfirmEmail confirmEmail = new ConfirmEmail
                {
                    IsActive = true,
                    ConfirmCode = GenerateCodeActive(),
                    ExpiryTime = DateTime.Now.AddMinutes(1),
                    IsConfirmed = false,
                    UserId = user.Id,
                };
                await _baseConfirmEmaiRepository.CreateAsync(confirmEmail);
                var message = new EmailMessage(new string[] { request.Email }, "Nhận mã xác nhận tại đây: ", $"Mã xác nhận: {confirmEmail.ConfirmCode}");
                var responseMessage = _emailService.SendEmail(message);
                return new ResponseObject<DataResponseUser>
                {
                    Status = StatusCodes.Status201Created,
                    Message = "Bạn đã gửi yêu cầu đăng ký! Vui lòng nhận mã xác nhận tại email để đăng ký tài khoản",
                    Data = _userConverter.EntityToDTO(user)
                };
            }
            catch (Exception ex)
            {
                return new ResponseObject<DataResponseUser>
                {
                    Status = StatusCodes.Status500InternalServerError,
                    Message = "Error: " + ex.Message,
                };
            }
        }

        private string GenerateCodeActive()
        {
            string str = "PhuongDong_" + DateTime.Now.Ticks.ToString();
            return str;
        }
    }
}


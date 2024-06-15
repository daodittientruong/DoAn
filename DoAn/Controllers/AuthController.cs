using Application.InterfaceService;
using Application.Payloads.RequestModels.UserRequests;
using Application.Payloads.ResponseModels.DataUsers;
using DoAn.Application.Constants;
using DoAn.Application.InterfaceService;
using DoAn.Application.Payloads.Response;
using DoAn.Application.Payloads.ResponseModels.DataUsers;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.StaticFiles;
using Microsoft.VisualBasic;

namespace DoAn.Controllers
{
    [Route(Constant.DefaultValues.DEFAULT_CONTROLLER_ROUTE)]
    [ApiController]
    public class AuthController : ControllerBase
    {
        private readonly IAuthService _authService;
        private readonly IUserService _userService;
        public AuthController(IAuthService authService, IUserService userService)
        {
            _authService = authService;
            _userService = userService;
        }
        [HttpPost]
        public async Task<IActionResult> Register([FromBody] Request_Register request)
        {
            return Ok(await _authService.Register(request));
        }
        [HttpPost]
        public async Task<IActionResult> ConfirmRegisterAccount(string confirmCode)
        {
            return Ok(await _authService.ConfirmRegisterAccount(confirmCode));
        }
        [HttpPost]
        public async Task<IActionResult> Login(Request_Login request)
        {
            return Ok(await _authService.Login(request));
        }
        [HttpPut]
        [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)] // Xác thực bằng json web bằng những thông tin mô tả có trong token và trong trường hợp này là Id   
        public async Task<IActionResult> ChangePassword([FromBody] Request_ChangePassword request)
        {
            long id = long.Parse(HttpContext.User.FindFirst("Id").Value);
            return Ok(await _authService.ChangePassword(id, request));
        }
        [HttpPost]
        public async Task<IActionResult> ForgotPassword(string email)
        {
            return Ok(await _authService.ForgotPassword(email));
        }
        [HttpPut]
        public async Task<IActionResult> ConfirmCreateNewPassword([FromBody] Request_CreateNewPassword request)
        {
            return Ok(await _authService.ConfirmCreateNewPassword(request));
        }
        [HttpPost("{userId}")]
        [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
        public async Task<IActionResult> AddRolesToUser([FromRoute] long userId, [FromBody] List<string> roles)
        {
            return Ok(await _authService.AddRolesToUser(userId, roles));
        }

        [HttpDelete("{userId}")]
        [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
        public async Task<IActionResult> DeleteRoles([FromRoute] long userId, [FromBody] List<string> roles)
        {
            return Ok(await _authService.DeleteRoles(userId, roles));
        }
        [HttpPut]
        [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
        public async Task<IActionResult> UpdateUser([FromForm] Request_UpdateUser request)
        {
            long Id = long.Parse(HttpContext.User.FindFirst("Id").Value);
            return Ok(await _userService.UpdateUser(Id, request));
        }
        [HttpGet]
        public async Task<IActionResult> DownloadFile(string fileName)
        {
            var filePath = Path.Combine(Directory.GetCurrentDirectory(), "Upload//Files", fileName);
            var provider = new FileExtensionContentTypeProvider();
            if(!provider.TryGetContentType(fileName, out var contentType))
            {
                contentType = "application/octet-stream";
            }
            var bytes = await System.IO.File.ReadAllBytesAsync(filePath);
            return File(bytes, contentType, Path.GetFileName(fileName));
        }
    }
}

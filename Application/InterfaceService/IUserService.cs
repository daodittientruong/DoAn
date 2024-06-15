using Application.Payloads.RequestModels.UserRequests;
using DoAn.Application.Payloads.Response;
using DoAn.Application.Payloads.ResponseModels.DataUsers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.InterfaceService
{
    public interface IUserService
    {
        Task<ResponseObject<DataResponseUser>> UpdateUser(long userId, Request_UpdateUser user);
    }
}

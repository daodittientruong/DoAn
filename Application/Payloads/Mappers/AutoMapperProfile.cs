using Application.Payloads.RequestModels.UserRequests;
using AutoMapper;
using DoAn.Application.Payloads.ResponseModels.DataUsers;
using DoAn.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Payloads.Mappers
{
    public class AutoMapperProfile : Profile
    {
        public AutoMapperProfile() 
        {
            CreateMap<Request_Register, User>();
            CreateMap<User, DataResponseUser>();
        }
    }
}

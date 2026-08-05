using AutoMapper;
using BackEnd.Modules.Auth.Dto;
using BackEnd.Modules.User.Dto;
using BackEnd.Modules.User.Entities;
using BackEnd.Utils.Dto;
using System.Net;

namespace BackEnd.Utils.Core
{
    public class MappingProfile : Profile
    {
        public MappingProfile()
        {
            CreateMap<ApplicationUser, UserResponse>();
            CreateMap<ApplicationUser, CurrentUserResponse>();
            CreateMap<UserRegisterRequest, ApplicationUser>();

            CreateMap<Exception, ErrorResponse>()
            .ForMember(dest => dest.Title, opt => opt.MapFrom(src => src.GetType().Name))
            .ForMember(dest => dest.Message, opt => opt.MapFrom(src => src.Message))
            .ForMember(dest => dest.StatusCode, opt => opt.MapFrom(src =>
                src is BadHttpRequestException ? (int)HttpStatusCode.BadRequest :
                src is UnauthorizedAccessException ? (int)HttpStatusCode.Unauthorized :
                src is KeyNotFoundException ? (int)HttpStatusCode.NotFound :
                (int)HttpStatusCode.InternalServerError
            ));
        }
    }
}
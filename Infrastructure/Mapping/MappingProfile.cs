using AutoMapper;
using BackEnd.Domain.Contracts;
using BackEnd.Domain.Entities;
using System.Net;

namespace BackEnd.Infrastructure.Mapping
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
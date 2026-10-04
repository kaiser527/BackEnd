using AutoMapper;
using BackEnd.Modules.Auth.Dto;
using BackEnd.Modules.History.Dto;
using BackEnd.Modules.History.Entities;
using BackEnd.Modules.QuizApp.Dto;
using BackEnd.Modules.QuizApp.Entities;
using BackEnd.Modules.QuizContent.Dto;
using BackEnd.Modules.QuizContent.Entities;
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
            CreateMap<UserLoginRequest, ApplicationUser>();
            CreateMap<UpdateUserProfileRequest, UserLoginRequest>();
            CreateMap<UpdateUserProfileRequest, ApplicationUser>()
                .ForMember(dest => dest.PasswordHash, opt => opt.Ignore());

            CreateMap<QuizRequest, Quiz>();
            CreateMap<Quiz, QuizResponse>()
                .ForMember(dest => dest.QuestionCount, opt => opt.MapFrom(src =>
                    src.Contents.SelectMany(c => c.Questions).Count()));

            CreateMap<Content, QuizContentResponse>();
            CreateMap<Question, QuestionResponse>();
            CreateMap<Answer, AnswerResponse>();

            CreateMap<QuizHistory, QuizHistoryResponse>();
            CreateMap<UserAnswer, UserAnswerResponse>();
            CreateMap<UserAnswerRequest, UserAnswer>();

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
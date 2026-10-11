using AutoMapper;
using BackEnd.Modules.Auth.Dto;
using BackEnd.Modules.History.Dto;
using BackEnd.Modules.History.Entities;
using BackEnd.Modules.Message.Dto;
using BackEnd.Modules.Message.Entities;
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
                    src.Contents.SelectMany(c => c.Questions).Count())
                );

            CreateMap<Content, QuizContentResponse>();
            CreateMap<Question, QuestionResponse>();
            CreateMap<Answer, AnswerResponse>();

            CreateMap<ApplicationUser, UserHistoryResponse>();
            CreateMap<UserAnswerRequest, UserAnswer>();
            CreateMap<UserAnswer, UserAnswerResponse>()
                .ForMember(dest => dest.QuestionText,opt => opt.MapFrom(src => src.Question.Text));
            CreateMap<QuizHistory, QuizHistoryResponse>()
                .ForMember(dest => dest.EssayCount, opt => opt.MapFrom(src =>
                    src.Quiz.Contents.SelectMany(c => c.Questions)
                        .Count(q => q.Type == QuestionType.Essay))
                );

            CreateMap<UserMessage, MessageResponse>();
            CreateMap<Conversation, ConversationResponse>();
            CreateMap<Conversation, ConversationListItem>();
            CreateMap<ApplicationUser, ConversationListUser>()
                .ForMember(dest => dest.Role, opt => opt.Ignore());

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
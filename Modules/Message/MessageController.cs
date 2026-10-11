using AutoMapper;
using BackEnd.Modules.Message.Dto;
using BackEnd.Utils.Dto;
using BackEnd.Utils.Helper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace BackEnd.Modules.Message
{
    [ApiController]
    [Route("api/[controller]")]
    public class MessageController(MessageService messageService, IMapper mapper) : ControllerBase
    {
        private readonly MessageService _messageService = messageService;
        private readonly IMapper _mapper = mapper;

        [HttpGet("{conversationId}")]
        [Authorize]
        public async Task<IActionResult> GetMessages(
            Guid conversationId,
            [FromQuery] string? cursor, 
            [FromQuery] int pageSize = 30)
        {
            return await ExceptionWrapper.Execute(async () =>
            {
                var history = await _messageService.GetMessages(
                    new MessageCursorRequest
                    {
                        ConversationId = conversationId,
                        Cursor = cursor
                    },
                    pageSize);
                var apiResponse = new ApiResponse<CursorPaginateResponse<MessageResponse>>
                {
                    StatusCode = 200,
                    Message = "Fetch messages successfully",
                    Result = history
                };
                return Ok(apiResponse);
            }, _mapper);
        }

        [HttpGet]
        [Authorize]
        public async Task<IActionResult> GetConversations(
            [FromQuery] string? cursor,
            [FromQuery] int pageSize = 10)
        {
            return await ExceptionWrapper.Execute(async () =>
            {
                var user = User;
                var isAdmin = user.IsInRole("Admin");
                var isStaff = user.IsInRole("Staff") || isAdmin;
                var conversations = await _messageService.GetConversations(
                    new ConversationCursorRequest
                    {
                        Cursor = cursor,
                        IsStaff = isStaff,
                        IsAdmin = isAdmin
                    },
                    pageSize);
                var apiResponse =
                    new ApiResponse<CursorPaginateResponse<ConversationListItem>>
                    {
                        StatusCode = 200,
                        Message = "Fetch conversations successfully",
                        Result = conversations
                    };
                return Ok(apiResponse);
            }, _mapper);
        }
    }
}

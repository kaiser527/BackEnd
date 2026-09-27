using AutoMapper;
using BackEnd.Modules.History.Dto;
using BackEnd.Utils.Dto;
using BackEnd.Utils.Helper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace BackEnd.Modules.History
{
    [ApiController]
    [Route("api/[controller]")]
    public class HistoryController(HistoryService historyService, IMapper mapper) : ControllerBase
    {
        private readonly HistoryService _historyService = historyService;
        private readonly IMapper _mapper = mapper;

        [HttpPost("{quizId}")]
        [Authorize]
        public async Task<IActionResult> CreateQuizHistory(Guid quizId)
        {
            return await ExceptionWrapper.Execute(async () =>
            {
                var history = await _historyService.CreateQuizHistory(quizId);
                var apiResponse = new ApiResponse<QuizHistoryResponse>
                {
                    StatusCode = 201,
                    Message = "Create quiz history successfully",
                    Result = history
                };
                return Ok(apiResponse);
            }, _mapper);
        }
    }
}

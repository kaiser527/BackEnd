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

        [HttpGet]
        [Authorize]
        public async Task<IActionResult> FetchQuizHistoriesPaginate([FromQuery] int pageNumber = 1, [FromQuery] int pageSize = 10)
        {
            return await ExceptionWrapper.Execute(async () =>
            {
                var response = await _historyService.FetchQuizHistoriesPaginate(pageNumber, pageSize);
                return Ok(new ApiResponse<PaginateReponse<QuizHistoryResponse>>
                {
                    StatusCode = 200,
                    Message = "Quiz histories fetched successfully",
                    Result = response
                });
            }, _mapper);
        }

        [HttpGet("avg-score")]
        [Authorize]
        public async Task<IActionResult> FetchHistoryAvgScore()
        {
            return await ExceptionWrapper.Execute(async () =>
            {
                var response = await _historyService.HistoryAvgScore();
                return Ok(new ApiResponse<HistoryAvgScoreResponse>
                {
                    StatusCode = 200,
                    Message = "Fetch history avg score successfully",
                    Result = response
                });
            }, _mapper);
        }

        [HttpGet("user-answer/{historyId}")]
        [Authorize]
        public async Task<IActionResult> FetchUserAnswers(Guid historyId)
        {
            return await ExceptionWrapper.Execute(async () =>
            {
                var response = await _historyService.FetchUserAnswers(historyId);
                return Ok(new ApiResponse<IEnumerable<UserAnswerResponse>>
                {
                    StatusCode = 200,
                    Message = "Fetch user answers successfully",
                    Result = response
                });
            }, _mapper);
        }

        [HttpPost("user-answer")]
        [Authorize]
        public async Task<IActionResult> UpsertUserAnswer([FromBody] UserAnswerRequest request)
        {
            return await ExceptionWrapper.Execute(async () =>
            {
                await _historyService.UpsertUserAnswer(request);
                return Ok(new { Status = 201, Message = "Upsert user answer successfully" });
            }, _mapper);
        }

        [HttpPost("submit/{historyId}")]
        [Authorize]
        public async Task<IActionResult> SubmitQuizHistory(Guid historyId)
        {
            return await ExceptionWrapper.Execute(async () =>
            {
                var response = await _historyService.SubmitQuiz(historyId);
                return Ok(new ApiResponse<QuizHistoryResponse>
                {
                    StatusCode = 201,
                    Message = "Submit quiz history successfully",
                    Result = response
                });
            }, _mapper);
        }

        [HttpPatch("grade-essays")]
        [Authorize(Roles = "Admin,Staff")]
        public async Task<IActionResult> GradeEssays([FromBody] GradeEssayRequest request)
        {
            return await ExceptionWrapper.Execute(async () =>
            {
                await _historyService.GradeEssays(request);
                return Ok(new { StatusCode = 201, Message = "Grade essay successfully" });
            }, _mapper);
        }

        [HttpGet("waiting")]
        [Authorize(Roles = "Admin,Staff")]
        public async Task<IActionResult> FetchWaitingHistories()
        {
            return await ExceptionWrapper.Execute(async () =>
            {
                var response = await _historyService.GetWaitingQuizHistories();
                return Ok(new ApiResponse<IEnumerable<QuizHistoryResponse>>
                {
                    StatusCode = 200,
                    Message = "Fetch waiting histories successfully",
                    Result = response
                });
            }, _mapper);
        }

        [HttpGet("waiting/{historyId}")]
        [Authorize(Roles = "Admin,Staff")]
        public async Task<IActionResult> FetchWaitingUserAnswersd(Guid historyId)
        {
            return await ExceptionWrapper.Execute(async () =>
            {
                var response = await _historyService.GetWaitingUserAnswers(historyId);
                return Ok(new ApiResponse<IEnumerable<UserAnswerResponse>>
                {
                    StatusCode = 200,
                    Message = "Fetch waiting user answers successfully",
                    Result = response
                });
            }, _mapper);
        }
    }
}

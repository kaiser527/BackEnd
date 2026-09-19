using AutoMapper;
using BackEnd.Modules.QuizApp.Dto;
using BackEnd.Utils.Dto;
using BackEnd.Utils.Helper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace BackEnd.Modules.QuizApp
{
    [ApiController]
    [Route("api/[controller]")]
    public class QuizController(QuizService quizService, IMapper mapper) : ControllerBase
    {
        private readonly QuizService _quizService = quizService;
        private readonly IMapper _mapper = mapper;

        [HttpPost]
        [Authorize(Roles = "Admin,Staff")]
        public async Task<IActionResult> Create([FromBody] QuizRequest request)
        {
            return await ExceptionWrapper.Execute(async () =>
            {
                var quiz = await _quizService.CreateQuiz(request);
                var quizResponse = _mapper.Map<QuizResponse>(quiz);
                var apiResponse = new ApiResponse<QuizResponse>
                {
                    StatusCode = 201,
                    Message = "Create quiz successfully",
                    Result = quizResponse
                };
                return Ok(apiResponse);
            }, _mapper);
        }

        [HttpPatch("{id}")]
        [Authorize(Roles = "Admin,Staff")]
        public async Task<IActionResult> Update(Guid id, [FromBody] QuizRequest request)
        {
            return await ExceptionWrapper.Execute(async () =>
            {
                var quiz = await _quizService.UpdateQuiz(id, request);
                var quizResponse = _mapper.Map<QuizResponse>(quiz);
                var apiResponse = new ApiResponse<QuizResponse>
                {
                    StatusCode = 201,
                    Message = "Create quiz successfully",
                    Result = quizResponse
                };
                return Ok(apiResponse);
            }, _mapper);
        }

        [HttpDelete("{id}")]
        [Authorize(Roles = "Admin,Staff")]
        public async Task<IActionResult> Delete(Guid id)
        {
            return await ExceptionWrapper.Execute(async () =>
            {
                await _quizService.DeleteQuiz(id);
                return Ok(new { Status = 200, Message = "Delete Quiz successfully" });
            }, _mapper);
        }

        [HttpGet]
        public async Task<IActionResult> GetQuizzes([FromQuery] int pageNumber = 1, [FromQuery] int pageSize = 10)
        {
            return await ExceptionWrapper.Execute(async () =>
            {
                var filter = new QuizFilterRequest
                {
                    Title = Request.Query["title"],
                    Type = Request.Query["type"],
                    Difficulty = Request.Query["difficulty"],
                };
                QsDateFilter.NormalizeDateFilter(Request, filter);
                var response = await _quizService.FetchQuizzesPaginate(filter, pageNumber, pageSize);
                return Ok(new ApiResponse<PaginateReponse<QuizResponse>>
                {
                    StatusCode = 200,
                    Message = "Quizzes fetched successfully",
                    Result = response
                });
            }, _mapper);
        }
    }
}

using AutoMapper;
using BackEnd.Modules.QuizContent.Dto;
using BackEnd.Utils.Dto;
using BackEnd.Utils.Helper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace BackEnd.Modules.QuizContent
{
    [ApiController]
    [Route("api/quiz-content")]
    public class QuizContentController(QuizContentService quizContentService, IMapper mapper) : ControllerBase
    {
        private readonly QuizContentService _quizContentService = quizContentService;
        private readonly IMapper _mapper = mapper;

        [HttpGet("{quizId}")]
        public async Task<IActionResult> FindContentsByQuiz(Guid quizId)
        {
            return await ExceptionWrapper.Execute(async () =>
            {
                var result = await _quizContentService.FindContentsByQuizId(quizId);
                var apiResponse = new ApiResponse<IEnumerable<QuizContentResponse>>
                {
                    StatusCode = 200,
                    Message = "Fetch contents by quizId successfully",
                    Result = result
                };
                return Ok(apiResponse);
            }, _mapper);
        }

        [HttpPost]
        [Authorize(Roles = "Admin,Staff")]
        public async Task<IActionResult> UpsertQuizContent([FromBody] QuizContentRequest request)
        {
            return await ExceptionWrapper.Execute(async () =>
            {
                var result = await _quizContentService.UpsertQuizContent(request);
                var apiResponse = new ApiResponse<IEnumerable<QuizContentResponse>>
                {
                    StatusCode = 201,
                    Message = "Upsert quiz content successfully",
                    Result = result
                };
                return Ok(apiResponse);
            }, _mapper);
        }
    }
}

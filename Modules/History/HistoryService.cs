using AutoMapper;
using BackEnd.Modules.Database;
using BackEnd.Modules.History.Dto;
using BackEnd.Modules.History.Entities;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace BackEnd.Modules.History
{
    public class HistoryService(
        ApplicationDbContext context, 
        IMapper mapper,
        IHttpContextAccessor httpContextAccessor)
    {
        private readonly ApplicationDbContext _context = context;
        private readonly IMapper _mapper = mapper;
        private readonly IHttpContextAccessor _httpContextAccessor = httpContextAccessor;

        public async Task<QuizHistoryResponse> CreateQuizHistory(Guid quizId)
        {
            var userId = 
                _httpContextAccessor.HttpContext?.User?.FindFirstValue(ClaimTypes.NameIdentifier) 
                ?? throw new BadHttpRequestException("User not exist");

            var existingHistory = await _context.QuizHistories
                .FirstOrDefaultAsync(h => h.UserId == userId && h.QuizId == quizId);

            if (existingHistory != null)
            {
                return _mapper.Map<QuizHistoryResponse>(existingHistory);
            }

            var entity = new QuizHistory
            {
                QuizId = quizId,
                UserId = userId,
                StartedAt = DateTime.UtcNow,
                Score = 0
            };

            await _context.QuizHistories.AddAsync(entity);
            await _context.SaveChangesAsync();

            return _mapper.Map<QuizHistoryResponse>(entity);
        }
    }
}

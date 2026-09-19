using AutoMapper;
using BackEnd.Modules.Database;
using BackEnd.Modules.QuizApp.Dto;
using BackEnd.Modules.QuizApp.Entities;
using BackEnd.Utils.Dto;
using Microsoft.EntityFrameworkCore;

namespace BackEnd.Modules.QuizApp
{
    public class QuizService(ApplicationDbContext context, IMapper mapper)
    {
        private readonly ApplicationDbContext _context = context;
        private readonly IMapper _mapper = mapper;

        public async Task<QuizResponse> CreateQuiz(QuizRequest request)
        {
            var exist = await _context.Quizzes.FirstOrDefaultAsync(q => q.Title == request.Title);

            if (exist != null)
            {
                throw new BadHttpRequestException("Quiz is already exist");
            }

            var entity = _mapper.Map<Quiz>(request);

            await _context.Quizzes.AddAsync(entity);
            await _context.SaveChangesAsync();

            return _mapper.Map<QuizResponse>(entity);
        }

        public async Task<QuizResponse> UpdateQuiz(Guid id, QuizRequest request)
        {
            var quiz = await _context.Quizzes
                .FindAsync(id) ?? throw new BadHttpRequestException("Quiz is not exist");

            bool isExist = await _context.Quizzes
                .AnyAsync(q => q.Title == request.Title && q.Id != id);

            if (isExist)
            {
                throw new BadHttpRequestException("Quiz title already exists");
            }

            _mapper.Map(request, quiz);

            await _context.SaveChangesAsync();

            return _mapper.Map<QuizResponse>(quiz);
        }

        public async Task DeleteQuiz(Guid id)
        {
            var quiz = await _context.Quizzes
                .FindAsync(id) ?? throw new BadHttpRequestException("Quiz is not exist");

            await _context.Quizzes.Where(c => c.Id == id).ExecuteDeleteAsync();
        }

        public async Task<PaginateReponse<QuizResponse>> FetchQuizzesPaginate(
            QuizFilterRequest request,
            int pageNumber = 1,
            int pageSize = 10
        )
        {
            var query = _context.Quizzes.AsQueryable();

            if (!string.IsNullOrWhiteSpace(request.Title))
                query = query.Where(u => u.Title.Contains(request.Title));

            if (!string.IsNullOrWhiteSpace(request.Type))
            {
                var types = request.Type
                    .Split(',', StringSplitOptions.RemoveEmptyEntries)
                    .Select(x => Enum.Parse<QuizType>(x.Trim(), true))
                    .ToList();

                query = query.Where(q => types.Contains(q.Type));
            }

            if (!string.IsNullOrWhiteSpace(request.Difficulty))
            {
                var difficulties = request.Difficulty
                    .Split(',', StringSplitOptions.RemoveEmptyEntries)
                    .Select(x => Enum.Parse<Difficulty>(x.Trim(), true))
                    .ToList();

                query = query.Where(q => difficulties.Contains(q.Difficulty));
            }

            if (request.CreatedAtRange != null && request.CreatedAtRange.Count == 2)
            {
                var start = request.CreatedAtRange[0];
                var end = request.CreatedAtRange[1];
                query = query.Where(u => u.CreatedAt >= start && u.CreatedAt <= end);
            }

            if (request.UpdatedAtRange != null && request.UpdatedAtRange.Count == 2)
            {
                var start = request.UpdatedAtRange[0];
                var end = request.UpdatedAtRange[1];
                query = query.Where(u => u.UpdatedAt >= start && u.UpdatedAt <= end);
            }

            if (request.SortByCreatedAt.HasValue)
                query = request.SortByCreatedAt.Value
                    ? query.OrderBy(u => u.CreatedAt)
                    : query.OrderByDescending(u => u.CreatedAt);

            if (request.SortByUpdatedAt.HasValue)
                query = request.SortByUpdatedAt.Value
                    ? query.OrderBy(u => u.UpdatedAt)
                    : query.OrderByDescending(u => u.UpdatedAt);

            var totalQuizzes = await query.CountAsync();
            var totalPages = (int)Math.Ceiling(totalQuizzes / (double)pageSize);

            var quizzes = await query
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            var quizResponses = quizzes.Select(q =>
            {
                var response = _mapper.Map<QuizResponse>(q);
                return response;
            }).ToList();

            var meta = new Meta
            {
                PageSize = pageSize,
                PageNumber = pageNumber,
                TotalPages = totalPages,
            };

            return new PaginateReponse<QuizResponse>
            {
                Meta = meta,
                Data = quizResponses
            };
        }
    }
}

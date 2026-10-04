using AutoMapper;
using BackEnd.Modules.Database;
using BackEnd.Modules.History.Dto;
using BackEnd.Modules.History.Entities;
using BackEnd.Modules.QuizContent.Entities;
using BackEnd.Utils.Dto;
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

            var quiz = await _context.Quizzes.FirstOrDefaultAsync(q => q.Id == quizId)
                ?? throw new KeyNotFoundException("Quiz not found");

            var existingHistory = await _context.QuizHistories
                .FirstOrDefaultAsync(h => h.UserId == userId && h.QuizId == quizId && !h.IsComplete);

            if (existingHistory != null)
            {
                return _mapper.Map<QuizHistoryResponse>(existingHistory);
            }

            var startedAt = DateTimeOffset.UtcNow;

            var entity = new QuizHistory
            {
                QuizId = quizId,
                UserId = userId,
                StartedAt = startedAt,
                ExpiresAt = startedAt.AddSeconds(quiz.TimeSeconds),
                IsComplete = false
            };

            await _context.QuizHistories.AddAsync(entity);
            await _context.SaveChangesAsync();

            return _mapper.Map<QuizHistoryResponse>(entity);
        }

        public async Task<PaginateReponse<QuizHistoryResponse>> FetchQuizHistoriesPaginate(
            int pageNumber = 1,
            int pageSize = 10
        )
        {
            var userId =
                _httpContextAccessor.HttpContext?.User?.FindFirstValue(ClaimTypes.NameIdentifier)
                ?? throw new BadHttpRequestException("User not exist");

            var query = _context.QuizHistories.Where(h => h.UserId == userId);

            var totalHistories = await query.CountAsync();
            var totalPages = (int)Math.Ceiling(totalHistories / (double)pageSize);

            var histories = await query
                .Include(h => h.Quiz)
                .OrderByDescending(h => h.StartedAt)
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            var historyResponses = histories.Select(h =>
            {
                var response = _mapper.Map<QuizHistoryResponse>(h);
                response.QuizTitle = h.Quiz.Title;
                return response;
            }).ToList();


            var meta = new Meta
            {
                PageSize = pageSize,
                PageNumber = pageNumber,
                TotalPages = totalPages,
                TotalCount = totalHistories
            };

            return new PaginateReponse<QuizHistoryResponse>
            {
                Meta = meta,
                Data = historyResponses
            };
        }

        public async Task<HistoryAvgScoreResponse> HistoryAvgScore()
        {
            var userId =
                _httpContextAccessor.HttpContext?.User?.FindFirstValue(ClaimTypes.NameIdentifier)
                ?? throw new BadHttpRequestException("User not exist");

            var averageScore = await _context.QuizHistories
                .Where(h =>
                    h.UserId == userId &&
                    h.IsComplete &&
                    h.Score != null)
                .Select(h => (double?)h.Score)
                .AverageAsync() ?? 0;

            return new HistoryAvgScoreResponse
            {
                AvgScore = Math.Round(averageScore, 2)
            };
        }

        public async Task<IEnumerable<UserAnswerResponse>> FetchUserAnswers(Guid historyId)
        {
            var userId =
                _httpContextAccessor.HttpContext?.User?.FindFirstValue(ClaimTypes.NameIdentifier)
                ?? throw new BadHttpRequestException("User not exist");

            var historyExists = await _context.QuizHistories
                .AnyAsync(h => h.Id == historyId && h.UserId == userId);

            if (!historyExists)
            {
                throw new BadHttpRequestException("Quiz history not found");
            }

            var userAnswers = await _context.UserAnswers
                .Where(ua => ua.QuizHistoryId == historyId)
                .OrderBy(ua => ua.CreatedAt)
                .ToListAsync();

            return _mapper.Map<IEnumerable<UserAnswerResponse>>(userAnswers);
        }

        public async Task UpsertUserAnswer(UserAnswerRequest request)
        {
            var userId =
                _httpContextAccessor.HttpContext?.User?.FindFirstValue(ClaimTypes.NameIdentifier)
                ?? throw new BadHttpRequestException("User not exist");

            var history = await _context.QuizHistories
                .FirstOrDefaultAsync(h =>
                    h.Id == request.QuizHistoryId &&
                    h.UserId == userId) ?? throw new KeyNotFoundException("Quiz history not found");

            var questionExists = await _context.Questions
                .AnyAsync(q =>
                    q.Id == request.QuestionId &&
                    q.Content.QuizId == history.QuizId);

            if (!questionExists)
            {
                throw new KeyNotFoundException("Question not found in this quiz");
            }

            var existingAnswer = await _context.UserAnswers
                .FirstOrDefaultAsync(ua =>
                    ua.QuizHistoryId == request.QuizHistoryId &&
                    ua.QuestionId == request.QuestionId);

            if (existingAnswer != null)
            {
                existingAnswer.SelectedAnswerId = request.SelectedAnswerId;
                existingAnswer.EssayAnswer = request.EssayAnswer;
            }
            else
            {
                var userAnswer = _mapper.Map<UserAnswer>(request);

                await _context.UserAnswers.AddAsync(userAnswer);
            }

            await _context.SaveChangesAsync();
        }

        public async Task<QuizHistoryResponse> SubmitQuiz(Guid historyId)
        {
            var userId =
                _httpContextAccessor.HttpContext?.User?.FindFirstValue(ClaimTypes.NameIdentifier)
                ?? throw new BadHttpRequestException("User not exist");

            var history = await _context.QuizHistories
                .Include(h => h.Quiz)
                    .ThenInclude(q => q.Contents)
                        .ThenInclude(c => c.Questions)
                            .ThenInclude(q => q.Answers)
                .Include(h => h.UserAnswers)
                    .ThenInclude(ua => ua.SelectedAnswer)
                .FirstOrDefaultAsync(h => h.Id == historyId && h.UserId == userId && !h.IsComplete)
                ?? throw new BadHttpRequestException("Quiz history not found");

            var questions = history.Quiz.Contents.SelectMany(c => c.Questions).ToList();
            var hasEssayQuestions = questions.Any(q => q.Type == QuestionType.Essay);

            // Make sure every question has a UserAnswer.
            foreach (var question in questions)
            {
                var userAnswer = history.UserAnswers.FirstOrDefault(a => a.QuestionId == question.Id);

                // Student didn't answer this question.
                if (userAnswer == null)
                {
                    userAnswer = new UserAnswer
                    {
                        QuizHistoryId = history.Id,
                        QuestionId = question.Id,
                        SelectedAnswerId = null,
                        EssayAnswer = null,
                        IsCorrect = question.Type == QuestionType.Essay ? null : false
                    };

                    history.UserAnswers.Add(userAnswer);

                    continue;
                }

                // Essay questions are always pending staff grading.
                if (question.Type == QuestionType.Essay)
                {
                    userAnswer.IsCorrect = null;
                    continue;
                }

                // Objective question was not answered.
                if (userAnswer.SelectedAnswerId == null)
                {
                    userAnswer.IsCorrect = false;
                    continue;
                }

                // Make sure the selected answer actually belongs
                // to this question.
                var selectedAnswer = question.Answers
                    .FirstOrDefault(a => a.Id == userAnswer.SelectedAnswerId);

                if (selectedAnswer == null)
                {
                    userAnswer.IsCorrect = false;
                    continue;
                }

                userAnswer.IsCorrect = selectedAnswer.IsCorrect;
            }

            history.IsComplete = true;
            history.SubmittedAt = DateTimeOffset.UtcNow;

            // No essays → calculate score immediately.
            if (!hasEssayQuestions)
            {
                var totalQuestions = questions.Count;
                var correctAnswers = history.UserAnswers.Count(a => a.IsCorrect == true);

                history.Score = totalQuestions == 0
                    ? 0
                    : Math.Round((double)correctAnswers / totalQuestions * 100, 2);
            }

            await _context.SaveChangesAsync();

            return _mapper.Map<QuizHistoryResponse>(history);
        }

        public async Task GradeEssays(GradeEssayRequest request)
        {
            if (request.Answers.Count == 0)
            {
                throw new BadHttpRequestException("No answers to grade");
            }

            var userAnswerIds = request.Answers
                .Select(x => x.UserAnswerId)
                .Distinct()
                .ToList();

            if (userAnswerIds.Count != request.Answers.Count)
            {
                throw new BadHttpRequestException("Duplicate answer IDs");
            }

            var userAnswers = await _context.UserAnswers
                .Include(a => a.QuizHistory)
                .Include(a => a.Question)
                .Where(a => userAnswerIds.Contains(a.Id))
                .ToListAsync();

            if (userAnswers.Count != userAnswerIds.Count)
            {
                throw new KeyNotFoundException("One or more answers not found");
            }

            var historyIds = userAnswers.Select(a => a.QuizHistoryId).Distinct().ToList();

            if (historyIds.Count != 1)
            {
                throw new BadHttpRequestException("All answers must belong to the same quiz history");
            }

            var history = userAnswers[0].QuizHistory;

            if (!history.IsComplete)
            {
                throw new BadHttpRequestException("Quiz has not been submitted");
            }

            foreach (var userAnswer in userAnswers)
            {
                if (userAnswer.Question.Type != QuestionType.Essay)
                {
                    throw new BadHttpRequestException("All selected answers must be essay questions");
                }

                var grade = request.Answers.First(x => x.UserAnswerId == userAnswer.Id);

                userAnswer.IsCorrect = grade.IsCorrect;
            }

            // Get all essay question IDs for this quiz.
            var essayQuestionIds = await _context.Questions
                .Where(q =>
                    q.Content.QuizId == history.QuizId &&
                    q.Type == QuestionType.Essay)
                .Select(q => q.Id)
                .ToListAsync();

            // Get all essay answers for this attempt.
            var essayAnswers = await _context.UserAnswers
                .Where(a =>
                    a.QuizHistoryId == history.Id &&
                    essayQuestionIds.Contains(a.QuestionId))
                .ToListAsync();

            // SubmitQuiz creates an answer for every question,
            // so every essay should have a UserAnswer.
            var allEssaysGraded = essayAnswers.All(a => a.IsCorrect.HasValue);

            if (allEssaysGraded)
            {
                var allAnswers = await _context.UserAnswers
                    .Where(a => a.QuizHistoryId == history.Id)
                    .ToListAsync();

                var totalQuestions = await _context.Questions
                    .Where(q => q.Content.QuizId == history.QuizId)
                    .CountAsync();

                var correctCount = allAnswers.Count(a => a.IsCorrect == true);

                history.Score = totalQuestions == 0
                    ? 0
                    : Math.Round((double)correctCount / totalQuestions * 100, 2);
            }

            await _context.SaveChangesAsync();
        }

        public async Task<IEnumerable<QuizHistoryResponse>> GetWaitingQuizHistories()
        {
            var histories = await _context.QuizHistories
                .Include(h => h.Quiz)
                    .ThenInclude(q => q.Contents)
                        .ThenInclude(c => c.Questions)
                .Where(h => h.IsComplete && h.Score == null)
                .ToListAsync();

            var responses = histories
                .Select(h =>
                {
                    var response = _mapper.Map<QuizHistoryResponse>(h);
                    response.EssayCount = h.Quiz.Contents
                        .SelectMany(c => c.Questions)
                        .Count(q => q.Type == QuestionType.Essay);
                    return response;
                })
                .ToList();

            return responses;
        }

        public async Task<IEnumerable<UserAnswerResponse>> GetWaitingUserAnswers(Guid historyId)
        {
            var history = await _context.QuizHistories
                .Include(h => h.UserAnswers)
                    .ThenInclude(a => a.Question)
                .FirstOrDefaultAsync(h => h.Id == historyId && h.IsComplete && h.Score == null)
                ?? throw new BadHttpRequestException("Quiz history not found or not waiting for grading");

            var waitingAnswers = history.UserAnswers
                .Where(a => a.Question.Type == QuestionType.Essay && a.IsCorrect == null)
                .ToList();

            return _mapper.Map<IEnumerable<UserAnswerResponse>>(waitingAnswers);
        }
    }
}

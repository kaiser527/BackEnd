using AutoMapper;
using BackEnd.Modules.Database;
using BackEnd.Modules.QuizContent.Dto;
using BackEnd.Modules.QuizContent.Entities;
using Microsoft.EntityFrameworkCore;

namespace BackEnd.Modules.QuizContent
{
    public class QuizContentService(ApplicationDbContext context, IMapper mapper)
    {
        private readonly ApplicationDbContext _context = context;
        private readonly IMapper _mapper = mapper;

        public async Task<IEnumerable<QuizContentResponse>> FindContentsByQuizId(Guid quizId)
        {
            var contents = await _context.Contents
                .Where(c => c.QuizId == quizId)
                .Include(c => c.Questions
                    .OrderBy(q => q.CreatedAt))
                    .ThenInclude(q => q.Answers
                        .OrderBy(a => a.CreatedAt))
                .OrderBy(c => c.CreatedAt)
                .ToListAsync();

            return _mapper.Map<IEnumerable<QuizContentResponse>>(contents);
        }

        public async Task<IEnumerable<QuizContentResponse>> UpsertQuizContent(QuizContentRequest request)
        {
            ValidateQuizContentRequest(request);

            await using var transaction = await _context.Database.BeginTransactionAsync();

            try
            {
                var quizExists = await _context.Quizzes.AnyAsync(q => q.Id == request.QuizId);

                if (!quizExists)
                {
                    throw new BadHttpRequestException("Quiz not found.");
                }

                var existingContents = await _context.Contents
                    .Where(c => c.QuizId == request.QuizId)
                    .Include(c => c.Questions)
                        .ThenInclude(q => q.Answers)
                    .ToListAsync();

                var contentChanged = HasQuizContentChanged(existingContents, request.Contents);

                if (contentChanged)
                {
                    var histories = await _context.QuizHistories
                        .Where(h => h.QuizId == request.QuizId)
                        .ToListAsync();

                    _context.QuizHistories.RemoveRange(histories);
                }

                var requestContentIds = request.Contents
                    .Where(c => c.Id != Guid.Empty)
                    .Select(c => c.Id)
                    .ToHashSet();

                var contentsToDelete = existingContents
                    .Where(c => !requestContentIds.Contains(c.Id))
                    .ToList();

                foreach (var content in contentsToDelete)
                {
                    foreach (var question in content.Questions)
                    {
                        _context.Answers.RemoveRange(question.Answers);
                    }
                    _context.Questions.RemoveRange(content.Questions);
                    _context.Contents.Remove(content);
                }

                foreach (var contentRequest in request.Contents)
                {
                    Content content;

                    if (contentRequest.Id == Guid.Empty)
                    {
                        content = new Content
                        {
                            Id = Guid.NewGuid(),
                            QuizId = request.QuizId,
                            Instruction = contentRequest.Instruction,
                            Passage = contentRequest.Passage,
                            AudioUrl = contentRequest.AudioUrl,
                            Image = contentRequest.Image
                        };
                        _context.Contents.Add(content);
                    }
                    else
                    {
                        content = existingContents
                            .FirstOrDefault(c => c.Id == contentRequest.Id)
                            ?? throw new BadHttpRequestException(
                                $"Content {contentRequest.Id} does not belong to this quiz."
                            );

                        content.Instruction = contentRequest.Instruction;
                        content.Passage = contentRequest.Passage;
                        content.AudioUrl = contentRequest.AudioUrl;
                        content.Image = contentRequest.Image;
                    }

                    var existingQuestions = content.Questions.ToList();

                    var requestQuestionIds = contentRequest.Questions
                        .Where(q => q.Id != Guid.Empty)
                        .Select(q => q.Id)
                        .ToHashSet();

                    var questionsToDelete = existingQuestions
                        .Where(q => !requestQuestionIds.Contains(q.Id))
                        .ToList();

                    foreach (var question in questionsToDelete)
                    {
                        _context.Answers.RemoveRange(question.Answers);
                        _context.Questions.Remove(question);
                    }

                    foreach (var questionRequest in contentRequest.Questions)
                    {
                        Question question;

                        if (questionRequest.Id == Guid.Empty)
                        {
                            question = new Question
                            {
                                Id = Guid.NewGuid(),
                                ContentId = content.Id,
                                Text = questionRequest.Text,
                                Type = questionRequest.Type
                            };
                            _context.Questions.Add(question);
                        }
                        else
                        {
                            question = existingQuestions
                                .FirstOrDefault(q => q.Id == questionRequest.Id)
                                ?? throw new BadHttpRequestException(
                                    $"Question {questionRequest.Id} does not belong to this content."
                                );

                            question.Text = questionRequest.Text;
                            question.Type = questionRequest.Type;
                        }

                        if (questionRequest.Type == QuestionType.Essay)
                        {
                            if (questionRequest.Answers.Count > 0)
                            {
                                throw new BadHttpRequestException(
                                    "Essay questions cannot have answers."
                                );
                            }
                            if (question.Answers.Count != 0)
                            {
                                _context.Answers.RemoveRange(question.Answers);
                            }
                            continue;
                        }

                        var existingAnswers = question.Answers.ToList();

                        var requestAnswerIds = questionRequest.Answers
                            .Where(a => a.Id != Guid.Empty)
                            .Select(a => a.Id)
                            .ToHashSet();

                        var answersToDelete = existingAnswers
                            .Where(a => !requestAnswerIds.Contains(a.Id))
                            .ToList();

                        _context.Answers.RemoveRange(answersToDelete);

                        foreach (var answerRequest in questionRequest.Answers)
                        {
                            if (answerRequest.Id == Guid.Empty)
                            {
                                var answer = new Answer
                                {
                                    Id = Guid.NewGuid(),
                                    QuestionId = question.Id,
                                    Text = answerRequest.Text,
                                    IsCorrect = answerRequest.IsCorrect
                                };
                                _context.Answers.Add(answer);
                            }
                            else
                            {
                                var answer = existingAnswers
                                    .FirstOrDefault(a => a.Id == answerRequest.Id)
                                    ?? throw new BadHttpRequestException(
                                        $"Answer {answerRequest.Id} does not belong to this question."
                                    );

                                answer.Text = answerRequest.Text;
                                answer.IsCorrect = answerRequest.IsCorrect;
                            }
                        }
                    }
                }

                await _context.SaveChangesAsync();
                await transaction.CommitAsync();

                var result = await _context.Contents
                    .Where(c => c.QuizId == request.QuizId)
                    .Include(c => c.Questions)
                        .ThenInclude(q => q.Answers)
                    .ToListAsync();

                return _mapper.Map<IEnumerable<QuizContentResponse>>(result);
            }
            catch
            {
                await transaction.RollbackAsync();
                throw;
            }
        }

        public Task<string[]> GetQuizContentImages()
        {
            return _context.Contents
                .Where(c => !string.IsNullOrEmpty(c.Image))
                .Select(c => c.Image ?? "")
                .ToArrayAsync();
        }

        public Task<string[]> GetQuizContentAudios()
        {
            return _context.Contents
                .Where(c => !string.IsNullOrEmpty(c.AudioUrl))
                .Select(c => c.AudioUrl ?? "")
                .ToArrayAsync();
        }

        private static void ValidateQuizContentRequest(QuizContentRequest request)
        {
            if (request.QuizId == Guid.Empty)
            {
                throw new BadHttpRequestException("QuizId is required.");
            }

            if (request.Contents.Count == 0)
            {
                throw new BadHttpRequestException(
                    "Quiz must contain at least one content."
                );
            }

            var contentIds = new HashSet<Guid>();

            foreach (var content in request.Contents)
            {
                if (string.IsNullOrWhiteSpace(content.Instruction))
                {
                    throw new BadHttpRequestException(
                        "Content instruction cannot be empty."
                    );
                }

                // Prevent duplicate content IDs
                if (content.Id != Guid.Empty && !contentIds.Add(content.Id))
                {
                    throw new BadHttpRequestException(
                        $"Duplicate content ID: {content.Id}."
                    );
                }

                if (content.Questions.Count == 0)
                {
                    throw new BadHttpRequestException(
                        "Each content must contain at least one question."
                    );
                }

                var questionIds = new HashSet<Guid>();

                foreach (var question in content.Questions)
                {
                    if (string.IsNullOrWhiteSpace(question.Text))
                    {
                        throw new BadHttpRequestException(
                            "Question text cannot be empty."
                        );
                    }

                    // Prevent duplicate question IDs
                    if (question.Id != Guid.Empty && !questionIds.Add(question.Id))
                    {
                        throw new BadHttpRequestException(
                            $"Duplicate question ID: {question.Id}."
                        );
                    }

                    switch (question.Type)
                    {
                        case QuestionType.Essay:

                            if (question.Answers.Count > 0)
                            {
                                throw new BadHttpRequestException(
                                    "Essay questions cannot have answers."
                                );
                            }

                            break;

                        case QuestionType.MultipleChoice:

                            ValidateChoiceAnswers( question.Answers, "Multiple choice");

                            break;

                        case QuestionType.TrueFalse:

                            if (question.Answers.Count != 2)
                            {
                                throw new BadHttpRequestException(
                                    "True/False questions must have exactly 2 answers."
                                );
                            }

                            ValidateAnswerText(question.Answers);

                            if (question.Answers.Count(a => a.IsCorrect) != 1)
                            {
                                throw new BadHttpRequestException(
                                    "True/False questions must have exactly 1 correct answer."
                                );
                            }

                            break;

                        default:
                            throw new BadHttpRequestException(
                                $"Unsupported question type: {question.Type}."
                            );
                    }
                }
            }
        }

        private static void ValidateChoiceAnswers(List<AnswerRequest> answers, string questionType)
        {
            if (answers.Count < 2)
            {
                throw new BadHttpRequestException(
                    $"{questionType} questions must have at least 2 answers."
                );
            }

            ValidateAnswerText(answers);

            if (!answers.Any(a => a.IsCorrect))
            {
                throw new BadHttpRequestException(
                    $"{questionType} questions must have at least 1 correct answer."
                );
            }
        }

        private static void ValidateAnswerText(List<AnswerRequest> answers)
        {
            foreach (var answer in answers)
            {
                if (string.IsNullOrWhiteSpace(answer.Text))
                {
                    throw new BadHttpRequestException(
                        "Answer text cannot be empty."
                    );
                }
            }

            var answerIds = new HashSet<Guid>();

            foreach (var answer in answers)
            {
                if (answer.Id != Guid.Empty && !answerIds.Add(answer.Id))
                {
                    throw new BadHttpRequestException(
                        $"Duplicate answer ID: {answer.Id}."
                    );
                }
            }
        }

        private static bool HasQuizContentChanged(List<Content> existingContents, IEnumerable<ContentRequest> requestContents)
        {
            var requestedContents = requestContents.ToList();

            if (existingContents.Count != requestedContents.Count)
                return true;

            foreach (var existingContent in existingContents)
            {
                var requestContent = requestedContents
                    .FirstOrDefault(c => c.Id == existingContent.Id);

                if (requestContent == null)
                    return true;

                if (existingContent.Instruction != requestContent.Instruction ||
                    existingContent.Passage != requestContent.Passage ||
                    existingContent.AudioUrl != requestContent.AudioUrl ||
                    existingContent.Image != requestContent.Image)
                {
                    return true;
                }

                var existingQuestions = existingContent.Questions.ToList();
                var requestedQuestions = requestContent.Questions.ToList();

                if (existingQuestions.Count != requestedQuestions.Count)
                    return true;

                foreach (var existingQuestion in existingQuestions)
                {
                    var requestQuestion = requestedQuestions
                        .FirstOrDefault(q => q.Id == existingQuestion.Id);

                    if (requestQuestion == null)
                        return true;

                    if (existingQuestion.Text != requestQuestion.Text ||
                        existingQuestion.Type != requestQuestion.Type)
                    {
                        return true;
                    }

                    var existingAnswers = existingQuestion.Answers.ToList();
                    var requestedAnswers = requestQuestion.Answers.ToList();

                    if (existingAnswers.Count != requestedAnswers.Count)
                        return true;

                    foreach (var existingAnswer in existingAnswers)
                    {
                        var requestAnswer = requestedAnswers
                            .FirstOrDefault(a => a.Id == existingAnswer.Id);

                        if (requestAnswer == null)
                            return true;

                        if (existingAnswer.Text != requestAnswer.Text ||
                            existingAnswer.IsCorrect != requestAnswer.IsCorrect)
                        {
                            return true;
                        }
                    }
                }
            }

            return false;
        }
    }
}

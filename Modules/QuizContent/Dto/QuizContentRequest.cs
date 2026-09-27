using BackEnd.Modules.QuizContent.Entities;

namespace BackEnd.Modules.QuizContent.Dto
{
    public class QuizContentRequest
    {
        public required Guid QuizId { get; set; }
        public List<ContentRequest> Contents { get; set; } = [];
    }

    public class AnswerRequest
    {
        public Guid Id { get; set; } = Guid.Empty;
        public required string Text { get; set; }
        public required bool IsCorrect { get; set; }
    }

    public class QuestionRequest
    {
        public Guid Id { get; set; } = Guid.Empty;
        public required string Text { get; set; }
        public required QuestionType Type { get; set; }
        public List<AnswerRequest> Answers { get; set; } = [];
    }

    public class ContentRequest
    {
        public Guid Id { get; set; } = Guid.Empty;
        public required string Instruction { get; set; }
        public string? Passage { get; set; }
        public string? AudioUrl { get; set; }
        public string? Image { get; set; }
        public List<QuestionRequest> Questions { get; set; } = [];
    }
}

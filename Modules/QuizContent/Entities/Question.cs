using BackEnd.Modules.History.Entities;
using BackEnd.Utils.Helper;

namespace BackEnd.Modules.QuizContent.Entities
{
    public enum QuestionType
    {
        MultipleChoice,
        TrueFalse,
        Essay
    }
    public class Question : IAuditableEntity
    {
        public Guid Id { get; set; }
        public required string Text { get; set; }
        public required QuestionType Type { get; set; }
        public Guid ContentId { get; set; }
        public Content Content { get; set; } = default!;
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }
        public ICollection<Answer> Answers { get; set; } = [];
        public ICollection<UserAnswer> UserAnswers { get; set; } = [];
    }
}

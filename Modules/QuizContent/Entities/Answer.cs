using BackEnd.Modules.History.Entities;

namespace BackEnd.Modules.QuizContent.Entities
{
    public class Answer
    {
        public Guid Id { get; set; }
        public required string Text { get; set; }
        public required bool IsCorrect { get; set; }
        public Guid QuestionId { get; set; }
        public Question Question { get; set; } = default!;
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }
        public ICollection<UserAnswer> UserAnswers { get; set; } = [];
    }
}

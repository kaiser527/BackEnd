using BackEnd.Modules.QuizContent.Entities;
using BackEnd.Utils.Helper;

namespace BackEnd.Modules.History.Entities
{
    public class UserAnswer : IAuditableEntity
    {
        public Guid Id { get; set; }
        public Guid QuizHistoryId { get; set; }
        public QuizHistory QuizHistory { get; set; } = default!;
        public Guid QuestionId { get; set; }
        public Question Question { get; set; } = default!;
        public Guid? SelectedAnswerId { get; set; }
        public Answer? SelectedAnswer { get; set; }
        public string? EssayAnswer { get; set; }
        public bool? IsCorrect { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }
    }
}

using BackEnd.Modules.QuizContent.Entities;

namespace BackEnd.Modules.History.Entities
{
    public class UserAnswer
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

using BackEnd.Modules.History.Entities;
using BackEnd.Modules.QuizContent.Entities;

namespace BackEnd.Modules.QuizApp.Entities
{
    public enum QuizType
    {
        Reading,
        Listening,
        Writing,
        Mixed,
    }
    public enum Difficulty
    {
        Easy,
        Medium,
        Hard,
    }
    public class Quiz
    {
        public Guid Id { get; set; }
        public required string Title { get; set; }
        public required string Image { get; set; }
        public required QuizType Type { get; set; }
        public required int TimeSeconds { get; set; }
        public required Difficulty Difficulty { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }
        public ICollection<Content> Contents { get; set; } = [];
        public ICollection<QuizHistory> QuizHistories { get; set; } = [];
    }
}

using BackEnd.Modules.QuizApp.Entities;

namespace BackEnd.Modules.QuizApp.Dto
{
    public class QuizResponse
    {
        public Guid Id { get; set; }
        public required string Title { get; set; }
        public required string Image { get; set; }
        public required QuizType Type { get; set; }
        public required int TimeSeconds { get; set; }
        public required Difficulty Difficulty { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }
    }
}

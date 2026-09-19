using BackEnd.Modules.QuizApp.Entities;

namespace BackEnd.Modules.QuizContent.Entities
{
    public class Content
    {      
        public Guid Id { get; set; }
        public required string Instruction { get; set; }
        public string? Passage { get; set; }
        public string? AudioUrl { get; set; }
        public string? Image { get; set; }
        public Guid QuizId { get; set; }
        public Quiz Quiz { get; set; } = default!;
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }
        public ICollection<Question> Questions { get; set; } = [];
    }
}

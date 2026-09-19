using BackEnd.Modules.QuizApp.Entities;

namespace BackEnd.Modules.QuizApp.Dto
{
    public class QuizRequest
    {
        public required string Title { get; set; }
        public required string Image { get; set; }
        public required QuizType Type { get; set; }
        public required int TimeSeconds { get; set; }
        public required Difficulty Difficulty { get; set; }
    }

    public class QuizFilterRequest
    {
        public string? Title { get; set; }
        public string? Type { get; set; }
        public string? Difficulty { get; set; }
        public bool? SortByCreatedAt { get; set; }
        public bool? SortByUpdatedAt { get; set; }
        public List<DateTime>? CreatedAtRange { get; set; }
        public List<DateTime>? UpdatedAtRange { get; set; }
    }
}

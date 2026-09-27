using BackEnd.Modules.QuizContent.Entities;

namespace BackEnd.Modules.QuizContent.Dto
{
    public class QuizContentResponse
    {
        public required Guid Id { get; set; }
        public required string Instruction { get; set; }
        public string? Passage { get; set; }
        public string? AudioUrl { get; set; }
        public string? Image { get; set; }
        public List<QuestionResponse> Questions { get; set; } = [];
    }

    public class QuestionResponse
    {
        public required Guid Id { get; set; }
        public required string Text { get; set; }
        public required QuestionType Type { get; set; }
        public List<AnswerResponse> Answers { get; set; } = [];
    }

    public class AnswerResponse
    {
        public required Guid Id { get; set; }
        public required string Text { get; set; }
        public required bool IsCorrect { get; set; }
    }
}

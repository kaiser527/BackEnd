namespace BackEnd.Modules.History.Dto
{
    public class QuizHistoryResponse
    {
        public Guid Id { get; set; }
        public double? Score { get; set; }
        public Guid QuizId { get; set; }
        public required string QuizTitle { get; set; }
        public required bool IsComplete { get; set; }
        public DateTimeOffset StartedAt { get; set; }
        public DateTimeOffset ExpiresAt { get; set; }
        public DateTimeOffset? SubmittedAt { get; set; }
        public int? EssayCount { get; set; }
        public List<UserAnswerResponse> UserAnswers { get; set; } = [];
    }

    public class HistoryAvgScoreResponse
    {
        public required double AvgScore { get; set; }
    }

    public class UserAnswerResponse
    {
        public Guid Id { get; set; }
        public Guid QuizHistoryId { get; set; }
        public Guid QuestionId { get; set; }
        public Guid? SelectedAnswerId { get; set; }
        public string? EssayAnswer { get; set; }
        public bool? IsCorrect { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }
    }
}

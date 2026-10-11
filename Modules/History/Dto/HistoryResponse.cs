using BackEnd.Modules.QuizApp.Entities;

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
        public required UserHistoryResponse User { get; set; }
        public List<UserAnswerResponse> UserAnswers { get; set; } = [];
    }

    public class UserHistoryResponse
    {
        public required string Id { get; set; }
        public required string Image { get; set; }
        public required string Email { get; set; }
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
        public string? QuestionText { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }
    }

    public class ProgressResponse
    {
        public List<PerformancePointResponse> Points { get; set; } = [];
        public List<GradeDistributionResponse> GradeDistribution { get; set; } = [];
        public SkillPerformanceResponse Overall { get; set; } = new();
        public LearningStreakResponse Streak { get; set; } = new();
    }

    public class PerformancePointResponse
    {
        public int Attempt { get; set; }
        public Guid QuizId { get; set; }
        public QuizType Type { get; set; }
        public double Score { get; set; }
        public DateTimeOffset SubmittedAt { get; set; }
    }

    public class SkillPerformanceResponse
    {
        public string? ReadingScore { get; set; }
        public string? WritingScore { get; set; }
        public string? ListeningScore { get; set; }
        public string? GrammarScore { get; set; }
        public string? OverallScore { get; set; }
    }

    public class LearningStreakResponse
    {
        public int CurrentStreak { get; set; }
        public List<StreakDayResponse> Week { get; set; } = [];
    }

    public class StreakDayResponse
    {
        public DateTime Date { get; set; }
        public bool Completed { get; set; }
    }

    public class GradeDistributionResponse
    {
        public string Grade { get; set; } = string.Empty;
        public int Count { get; set; }
        public double Percentage { get; set; }
    }
}

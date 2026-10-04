using BackEnd.Modules.QuizApp.Entities;
using BackEnd.Modules.User.Entities;

namespace BackEnd.Modules.History.Entities
{
    public class QuizHistory
    {
        public Guid Id { get; set; }
        public required string UserId { get; set; }
        public ApplicationUser User { get; set; } = default!;
        public Guid QuizId { get; set; }
        public Quiz Quiz { get; set; } = default!;
        public DateTimeOffset StartedAt { get; set; }
        public DateTimeOffset ExpiresAt { get; set; }
        public DateTimeOffset? SubmittedAt { get; set; }
        public bool IsComplete { get; set; } = false;
        public double? Score { get; set; }
        public ICollection<UserAnswer> UserAnswers { get; set; } = [];
    }
}

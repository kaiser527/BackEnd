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
        public DateTime StartedAt { get; set; }
        public DateTime SubmittedAt { get; set; }
        public double Score { get; set; }
        public ICollection<UserAnswer> UserAnswers { get; set; } = [];
    }
}

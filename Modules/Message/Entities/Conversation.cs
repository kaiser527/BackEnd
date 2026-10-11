using BackEnd.Modules.User.Entities;

namespace BackEnd.Modules.Message.Entities
{
    public enum ConversationStatus
    {
        Open,
        Closed
    }

    public class Conversation
    {
        public Guid Id { get; set; }
        public string UserId { get; set; } = null!;
        public ApplicationUser User { get; set; } = null!;
        public string? StaffId { get; set; }
        public ApplicationUser? Staff { get; set; }
        public ConversationStatus Status { get; set; } = ConversationStatus.Open;
        public DateTimeOffset CreatedAt { get; set; }
        public DateTimeOffset? ClosedAt { get; set; }
        public DateTimeOffset LastMessageAt { get; set; }
        public ICollection<UserMessage> Messages { get; set; } = [];
    }
}

using BackEnd.Modules.User.Entities;

namespace BackEnd.Modules.Message.Entities
{
    public class UserMessage
    {
        public Guid Id { get; set; }
        public Guid ConversationId { get; set; }
        public Conversation Conversation { get; set; } = null!;
        public string SenderId { get; set; } = null!;
        public ApplicationUser Sender { get; set; } = null!;
        public string Content { get; set; } = null!;
        public DateTimeOffset CreatedAt { get; set; }
        public DateTimeOffset? ReadAt { get; set; }
    }
}

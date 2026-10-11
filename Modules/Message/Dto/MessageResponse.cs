using BackEnd.Modules.Message.Entities;

namespace BackEnd.Modules.Message.Dto
{
    public class MessageResponse
    {
        public Guid Id { get; set; }
        public string SenderId { get; set; } = null!;
        public Guid ConversationId { get; set; }
        public string Content { get; set; } = null!;
        public string? SenderImage { get; set; }
        public DateTimeOffset CreatedAt { get; set; }
        public DateTimeOffset? ReadAt { get; set; }
    }

    public class ConversationResponse
    {
        public Guid Id { get; set; }
        public string? StaffId { get; set; }
        public ConversationStatus Status { get; set; } = ConversationStatus.Open;
        public DateTimeOffset CreatedAt { get; set; }
        public DateTimeOffset? ClosedAt { get; set; }
        public DateTimeOffset LastMessageAt { get; set; }
    }

    public class ConversationListItem
    {
        public Guid Id { get; set; }
        public ConversationStatus Status { get; set; }
        public DateTimeOffset CreatedAt { get; set; }
        public DateTimeOffset? ClosedAt { get; set; }
        public DateTimeOffset LastMessageAt { get; set; }
        public string? LastMessageContent { get; set; }
        public ConversationListUser? User { get; set; }
        public ConversationListUser? Participant { get; set; }
    }

    public class ConversationListUser
    {
        public required string Id { get; set; }
        public required string FirstName { get; set; }
        public required string LastName { get; set; }
        public required string Email { get; set; }
        public required string Image { get; set; }
        public required string Role { get; set; }
    }
}

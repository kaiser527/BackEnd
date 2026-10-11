namespace BackEnd.Modules.Message.Dto
{
    public class ChatRequest
    {
        public Guid? ConversationId { get; set; }
        public required string Content { get; set; }
    }

    public class ConversationCursorRequest
    {
        public required bool IsStaff { get; set; }
        public required bool IsAdmin { get; set; }
        public string? Cursor { get; set; }
    }

    public class MessageCursorRequest
    {
        public Guid ConversationId { get; set; }
        public string? Cursor { get; set; }
    }
}

using BackEnd.Modules.Message.Dto;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;
using System.Security.Claims;

namespace BackEnd.Modules.Message
{
    [Authorize]
    public class ChatHub(MessageService messageService) : Hub
    {
        private readonly MessageService _messageService = messageService;

        public async Task<MessageResponse> Chat(ChatRequest request)
        {
            var user = Context.User
                ?? throw new HubException("User not authenticated.");

            var userId = user.FindFirstValue(ClaimTypes.NameIdentifier)
                ?? throw new HubException("User ID not found.");

            if (user.IsInRole("Staff") || user.IsInRole("Admin"))
            {
                throw new HubException("Staff and admins cannot initiate a help conversation.");
            }

            if (string.IsNullOrWhiteSpace(request.Content))
                throw new HubException("Message content cannot be empty.");

            var conversationId = await _messageService.GetOrCreateConversationId(
                userId,
                request.ConversationId,
                isStaff: false);

            await JoinConversation(conversationId);

            return await _messageService.SendMessageToConversation(request, conversationId, userId);
        }

        public async Task<MessageResponse> Reply(ChatRequest request)
        {
            var user = Context.User
                ?? throw new HubException("User not authenticated.");

            var userId = user.FindFirstValue(ClaimTypes.NameIdentifier)
                ?? throw new HubException("User ID not found.");

            if (!user.IsInRole("Staff") && !user.IsInRole("Admin"))
                throw new HubException("Only staff or admins can reply here.");

            if (string.IsNullOrWhiteSpace(request.Content))
                throw new HubException("Message content cannot be empty.");

            if (request.ConversationId is null || request.ConversationId == Guid.Empty)
                throw new HubException("Conversation ID is required to reply.");

            var conversationId = request.ConversationId.Value;

            await JoinConversation(conversationId);

            return await _messageService.SendMessageToConversation(request, conversationId, userId);
        }

        public async Task JoinConversation(Guid conversationId)
        {
            var userId = Context.User?
                .FindFirstValue(ClaimTypes.NameIdentifier)
                ?? throw new HubException("User not authenticated.");

            var allowed = await _messageService.CanAccessConversation(conversationId, userId);

            if (!allowed)
                throw new HubException("Conversation not found or access denied.");

            await Groups.AddToGroupAsync(Context.ConnectionId, conversationId.ToString());
        }

        public async Task LeaveConversation(Guid conversationId)
        {
            var userId = Context.User?.FindFirstValue(ClaimTypes.NameIdentifier)
                ?? throw new HubException("User not authenticated.");

            var allowed = await _messageService.CanAccessConversation(
                conversationId, userId);

            if (!allowed)
                throw new HubException("Conversation not found or access denied.");

            await Groups.RemoveFromGroupAsync(Context.ConnectionId, conversationId.ToString());
        }

        public async Task CloseConversation(Guid conversationId)
        {
            var user = Context.User
                ?? throw new HubException("User not authenticated.");

            if (!user.IsInRole("Staff") && !user.IsInRole("Admin"))
                throw new HubException("Only staff or admins can close conversations.");

            var userId = user.FindFirstValue(ClaimTypes.NameIdentifier)
                ?? throw new HubException("User ID not found.");

            var closed = await _messageService.CloseConversation(conversationId, userId, user.IsInRole("Admin"));

            if (!closed)
                throw new HubException("Conversation not found or access denied.");

            await Clients.Group(conversationId.ToString()).SendAsync("ConversationClosed", conversationId);
        }

        public async Task<int> MarkMessagesAsRead(Guid conversationId)
        {
            var userId = Context.User?.FindFirstValue(ClaimTypes.NameIdentifier)
                ?? throw new HubException("User not authenticated.");

            return await _messageService.MarkMessagesAsRead(conversationId, userId);
        }
    }
}

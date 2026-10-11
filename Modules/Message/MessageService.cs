using AutoMapper;
using BackEnd.Modules.Database;
using BackEnd.Modules.Message.Dto;
using BackEnd.Modules.Message.Entities;
using BackEnd.Modules.User.Entities;
using BackEnd.Utils.Dto;
using BackEnd.Utils.Helper;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace BackEnd.Modules.Message
{
    public class MessageService(
        ApplicationDbContext context,
        IMapper mapper,
        IHubContext<ChatHub> hubContext,
        IHttpContextAccessor httpContextAccessor)
    {
        private readonly ApplicationDbContext _context = context;
        private readonly IMapper _mapper = mapper;
        private readonly IHubContext<ChatHub> _hubContext = hubContext;
        private readonly IHttpContextAccessor _httpContextAccessor = httpContextAccessor;

        private async Task<Conversation> GetOrCreateConversation(string userId, Guid? conversationId, bool isStaff)
        {
            if (conversationId.HasValue)
            {
                var existing = await _context.Conversations
                    .FirstOrDefaultAsync(x =>
                        x.Id == conversationId.Value && (x.UserId == userId || x.StaffId == userId));

                return existing is null
                    ? throw new HubException("Conversation not found or access denied.")
                    : existing;
            }

            if (isStaff)
                throw new HubException("Staff must specify an assigned conversation.");

            // Reuse the customer's existing open conversation.
            var openConversation = await _context.Conversations
                .Where(x =>
                    x.UserId == userId &&
                    x.Status == ConversationStatus.Open)
                .OrderByDescending(x => x.LastMessageAt)
                .FirstOrDefaultAsync();

            if (openConversation is not null)
                return openConversation;

            // Create a new conversation only if none is open.
            var now = DateTimeOffset.UtcNow;

            var conversation = new Conversation
            {
                Id = Guid.NewGuid(),
                UserId = userId,
                Status = ConversationStatus.Open,
                CreatedAt = now,
                LastMessageAt = now
            };

            _context.Conversations.Add(conversation);

            try
            {
                await _context.SaveChangesAsync();
                return conversation;
            }
            catch (DbUpdateException)
            {
                _context.Entry(conversation).State = EntityState.Detached;

                var existingConversation = await _context.Conversations
                    .AsNoTracking()
                    .FirstOrDefaultAsync(x => x.UserId == userId && x.Status == ConversationStatus.Open);

                if (existingConversation is not null)
                    return existingConversation;

                throw;
            }
        }

        public async Task<Guid> GetOrCreateConversationId(string userId, Guid? conversationId, bool isStaff)
        {
            var conversation = await GetOrCreateConversation(userId, conversationId, isStaff);

            if (conversation.Status != ConversationStatus.Open)
                throw new HubException("This conversation is closed.");

            return conversation.Id;
        }

        public async Task<MessageResponse> SendMessageToConversation(ChatRequest request, Guid conversationId, string userId)
        {
            if (string.IsNullOrWhiteSpace(request.Content))
                throw new HubException("Message content cannot be empty.");

            var conversation = await _context.Conversations
                .FirstOrDefaultAsync(x =>
                    x.Id == conversationId &&
                    (x.UserId == userId || x.StaffId == userId)) 
                ?? throw new HubException("Conversation not found or access denied.");

            if (conversation.Status != ConversationStatus.Open)
                throw new HubException("This conversation is closed.");

            var now = DateTimeOffset.UtcNow;

            var message = new UserMessage
            {
                Id = Guid.NewGuid(),
                ConversationId = conversation.Id,
                SenderId = userId,
                Content = request.Content.Trim(),
                CreatedAt = now
            };

            conversation.LastMessageAt = now;

            _context.Messages.Add(message);
            await _context.SaveChangesAsync();

            var response = _mapper.Map<MessageResponse>(message);

            await _hubContext.Clients
                .Group(conversation.Id.ToString())
                .SendAsync("ReceiveMessage", response);

            return response;
        }

        public async Task<bool> CanAccessConversation(Guid conversationId, string userId)
        {
            return await _context.Conversations
                .AnyAsync(x =>
                    x.Id == conversationId &&
                    (x.UserId == userId || x.StaffId == userId));
        }

        public async Task<bool> CloseConversation(Guid conversationId, string userId, bool isAdmin)
        {
            var conversation = await _context.Conversations
                .FirstOrDefaultAsync(x =>
                    x.Id == conversationId &&
                    (isAdmin || x.StaffId == userId));

            if (conversation is null)
                return false;

            if (conversation.Status == ConversationStatus.Closed)
                return true;

            conversation.Status = ConversationStatus.Closed;
            conversation.ClosedAt = DateTimeOffset.UtcNow;

            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<int> MarkMessagesAsRead(Guid conversationId, string userId)
        {
            var canAccess = await CanAccessConversation(conversationId, userId);

            if (!canAccess)
                throw new HubException("Conversation not found or access denied.");

            // Find unread messages sent by the other participant.
            var unreadMessages = await _context.Messages
                .Where(m =>
                    m.ConversationId == conversationId &&
                    m.SenderId != userId &&
                    m.ReadAt == null)
                .ToListAsync();

            if (unreadMessages.Count == 0)
                return 0;

            var readAt = DateTimeOffset.UtcNow;

            foreach (var message in unreadMessages)
            {
                message.ReadAt = readAt;
            }

            await _context.SaveChangesAsync();

            // Notify everyone currently connected to this conversation.
            await _hubContext.Clients
                .Group(conversationId.ToString())
                .SendAsync("MessagesRead", new
                {
                    ConversationId = conversationId,
                    ReaderId = userId,
                    MessageIds = unreadMessages.Select(m => m.Id).ToList(),
                    ReadAt = readAt
                });

            return unreadMessages.Count;
        }
        public async Task<CursorPaginateResponse<ConversationListItem>> GetConversations(
            ConversationCursorRequest request,
            int pageSize = 10)
        {
            var userId = _httpContextAccessor.HttpContext?.User
                ?.FindFirstValue(ClaimTypes.NameIdentifier)
                ?? throw new BadHttpRequestException("User not authenticated.");

            pageSize = Math.Clamp(pageSize, 1, 50);

            var query = _context.Conversations
                .AsNoTracking()
                .Include(c => c.User)
                .Include(c => c.Staff)
                .AsQueryable();

            // Filter conversations based on the current viewer.
            if (!request.IsAdmin)
            {
                query = request.IsStaff
                    ? query.Where(c => c.StaffId == userId)
                    : query.Where(c => c.UserId == userId);
            }

            // Cursor pagination.
            var cursor = CursorHelper.Decode(request.Cursor);

            if (cursor is { } position)
            {
                var timestamp = position.Timestamp;
                var id = position.Id;

                query = query.Where(c =>
                    c.LastMessageAt < timestamp ||
                    (c.LastMessageAt == timestamp && c.Id.CompareTo(id) < 0));
            }

            // Fetch one extra conversation to determine HasMore.
            var conversations = await query
                .OrderByDescending(c => c.LastMessageAt)
                .ThenByDescending(c => c.Id)
                .Take(pageSize + 1)
                .ToListAsync();

            var hasMore = conversations.Count > pageSize;

            if (hasMore)
                conversations.RemoveAt(pageSize);

            var items = _mapper.Map<List<ConversationListItem>>(conversations);

            var participantIds = conversations
                .SelectMany(c => new[] { c.User?.Id, c.Staff?.Id })
                .Where(id => id != null)
                .Select(id => id!)
                .Distinct()
                .ToList();

            var roleRows = await (
                from userRole in _context.UserRoles
                join role in _context.Roles
                    on userRole.RoleId equals role.Id
                where participantIds.Contains(userRole.UserId)
                select new
                {
                    userRole.UserId,
                    RoleName = role.Name
                })
                .ToListAsync();

            var roleLookup = roleRows
                .GroupBy(r => r.UserId)
                .ToDictionary(
                    g => g.Key,
                    g => g.Select(r => r.RoleName)
                        .Where(name => name != null)
                        .OrderBy(name => name)
                        .FirstOrDefault() ?? "User");

            for (var i = 0; i < conversations.Count; i++)
            {
                var conversation = conversations[i];
                var item = items[i];

                var otherUser = request.IsStaff || request.IsAdmin
                    ? conversation.User
                    : conversation.Staff;

                if (otherUser == null)
                {
                    item.Participant = null;
                    continue;
                }

                var role = roleLookup.TryGetValue(otherUser.Id, out var foundRole)
                    ? foundRole
                    : "User";

                item.Participant = new ConversationListUser
                {
                    Id = otherUser.Id,
                    FirstName = otherUser.FirstName,
                    LastName = otherUser.LastName,
                    Email = otherUser.Email ?? string.Empty,
                    Image = otherUser.Image,
                    Role = role
                };
            }

            var conversationIds = conversations
                .Select(c => c.Id)
                .ToList();

            if (conversationIds.Count > 0)
            {
                var lastMessages = await _context.Messages
                    .AsNoTracking()
                    .Where(m => conversationIds.Contains(m.ConversationId))
                    .GroupBy(m => m.ConversationId)
                    .Select(g => g
                        .OrderByDescending(m => m.CreatedAt)
                        .ThenByDescending(m => m.Id)
                        .Select(m => new
                        {
                            m.ConversationId,
                            m.Content
                        })
                        .First())
                    .ToListAsync();

                var lastMessageLookup = lastMessages.ToDictionary(
                    m => m.ConversationId,
                    m => m.Content);

                foreach (var item in items)
                {
                    item.LastMessageContent = lastMessageLookup.GetValueOrDefault(item.Id);
                }
            }

            var nextCursor = hasMore && conversations.Count > 0
                ? CursorHelper.Encode(conversations[^1].LastMessageAt, conversations[^1].Id)
                : null;

            return new CursorPaginateResponse<ConversationListItem>
            {
                Data = items,
                NextCursor = nextCursor,
                HasMore = hasMore
            };
        }

        public async Task<CursorPaginateResponse<MessageResponse>> GetMessages(
            MessageCursorRequest request,
            int pageSize = 30)
        {
            var userId = _httpContextAccessor.HttpContext?.User
                ?.FindFirstValue(ClaimTypes.NameIdentifier)
                ?? throw new BadHttpRequestException("User not authenticated.");

            pageSize = Math.Clamp(pageSize, 1, 50);

            var canAccess = await _context.Conversations
                .AnyAsync(c =>
                    c.Id == request.ConversationId &&
                    (c.UserId == userId || c.StaffId == userId));

            if (!canAccess)
                throw new BadHttpRequestException("Conversation not found or access denied.");

            var query = _context.Messages
                .AsNoTracking()
                .Include(m => m.Sender)
                .Where(m => m.ConversationId == request.ConversationId);

            var decoded = CursorHelper.Decode(request.Cursor);

            if (decoded is { } position)
            {
                var timestamp = position.Timestamp;
                var id = position.Id;

                query = query.Where(m =>
                    m.CreatedAt < timestamp ||
                    (m.CreatedAt == timestamp && m.Id.CompareTo(id) < 0));
            }

            var messages = await query
                .OrderByDescending(m => m.CreatedAt)
                .ThenByDescending(m => m.Id)
                .Take(pageSize + 1)
                .ToListAsync();

            var hasMore = messages.Count > pageSize;

            if (hasMore)
                messages.RemoveAt(pageSize);

            var nextCursor = hasMore && messages.Count > 0
                ? CursorHelper.Encode(messages[^1].CreatedAt, messages[^1].Id)
                : null;

            var items = _mapper.Map<List<MessageResponse>>(messages);

            for (var i = 0; i < messages.Count; i++)
            {
                var message = messages[i];

                items[i].SenderImage = message.SenderId == userId
                    ? null
                    : message.Sender?.Image;
            }

            items.Reverse();

            return new CursorPaginateResponse<MessageResponse>
            {
                Data = items,
                NextCursor = nextCursor,
                HasMore = hasMore
            };
        }
    }
}
using System.ComponentModel.DataAnnotations;

namespace BackEnd.Domain.Entities
{
    public class ChatRoom
    {
        [Key]
        public int Id { get; set; }

        [MaxLength(50)]
        public required string Name { get; set; }

        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }

        public ICollection<Message> Messages { get; set; } = [];
        public ICollection<ChatRoomUser> ChatRoomUsers { get; set; } = [];
    }
}

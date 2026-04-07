using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace BackEnd.Domain.Entities
{
    public class Message
    {
        [Key]
        public int Id { get; set; }

        [MaxLength(200)]    
        public required string Content { get; set; }

        public required string UserId { get; set; } 

        [ForeignKey("UserId")]
        public required ApplicationUser User { get; set; }

        public required int ChatRoomId { get; set; }

        [ForeignKey("ChatRoomId")]
        public required ChatRoom ChatRoom { get; set; }

        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }
    }
}

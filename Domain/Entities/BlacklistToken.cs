using System.ComponentModel.DataAnnotations;

namespace BackEnd.Domain.Entities
{
    public class BlacklistToken
    {
        [Key]
        public int Id { get; set; }
        public required string Jti { get; set; }
        public required DateTime ExpiryTime { get; set; }
    }
}

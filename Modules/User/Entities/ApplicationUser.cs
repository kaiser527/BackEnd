using BackEnd.Modules.History.Entities;
using Microsoft.AspNetCore.Identity;

namespace BackEnd.Modules.User.Entities
{
    public class ApplicationUser : IdentityUser
    {  
        public required string FirstName { get; set; }
        public required string LastName { get; set; }
        public required string Gender { get; set; }
        public required string Image { get; set; } = "user.png";
        public string? RefreshToken { get; set; }
        public DateTime? RefreshTokenExpiryTime { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }
        public ICollection<QuizHistory> QuizHistories { get; set; } = [];
    }
}

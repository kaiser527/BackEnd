using Microsoft.AspNetCore.Identity;

namespace BackEnd.Modules.User.Entities
{
    public class ApplicationUser : IdentityUser
    {  
        public required string FirstName { get; set; }
        public required string LastName { get; set; }
        public required string Gender { get; set; }
        public string? RefreshToken { get; set; }
        public DateTime? RefreshTokenExpiryTime { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }
    }
}

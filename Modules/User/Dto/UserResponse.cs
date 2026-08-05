namespace BackEnd.Modules.User.Dto
{
    public class UserResponse
    {
        public Guid Id { get; set; }
        public required string FirstName { get; set; }
        public required string LastName { get; set; }
        public required string Email { get; set; }
        public required string Gender { get; set; }
        public required string Role { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }
        public string? RefreshToken { get; set; }
        public string? AccessToken { get; set; }
    }
}

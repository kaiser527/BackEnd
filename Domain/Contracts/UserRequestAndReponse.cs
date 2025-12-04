namespace BackEnd.Domain.Contracts
{
    public class UserRegisterRequest
    {
        public required string FirstName { get; set; }
        public required string LastName { get; set; }
        public required string Email { get; set; }
        public required string Password { get; set; }
        public required string Gender { get; set; }
    }

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

    public class UserLoginRequest
    {
        public required string Email { get; set; }
        public required string Password { get; set; }
    }

    public class CurrentUserResponse
    {
        public required string FirstName { get; set; }
        public required string LastName { get; set; }
        public required string Email { get; set; }
        public required string Gender { get; set; }
        public required string Role { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }
        public string? AccessToken { get; set; }
    }

    public class UpdateUserRequest
    {
        public required string FirstName { get; set; }
        public required string LastName { get; set; }
        public required string Email { get; set; }
        public required string Gender { get; set; }
        public required string Password { get; set; }
        public required string Role { get; set; }
    }

    public class RevokeRefreshTokenResponse
    {
        public required string Message { get; set; }
    }

    public class RefreshTokenRequest
    {
        public required string RefreshToken { get; set; }
    }

    public class UserFilterRequest
    {
        public string? Email { get; set; }
        public string? FirstName { get; set; }
        public string? LastName { get; set; }
        public string? Role { get; set; }
        public bool? SortByCreatedAt { get; set; }
        public bool? SortByUpdatedAt { get; set; }
        public List<DateTime>? CreatedAtRange { get; set; }
        public List<DateTime>? UpdatedAtRange { get; set; }
    }
}

namespace BackEnd.Modules.Auth.Dto
{
    public class RefreshTokenRequest
    {
        public required string RefreshToken { get; set; }
    }

    public class UserRegisterRequest
    {
        public required string FirstName { get; set; }
        public required string LastName { get; set; }
        public required string Email { get; set; }
        public required string Password { get; set; }
        public required string Gender { get; set; }
    }

    public class UpdateUserProfileRequest
    {
        public string FirstName { get; set; } = "";
        public string LastName { get; set; } = "";
        public string Email { get; set; } = "";
        public string? Password { get; set; }
        public string Gender { get; set; } = "";
        public string Image { get; set; } = "";
    }

    public class UserLoginRequest
    {
        public required string Email { get; set; }
        public required string Password { get; set; }
    }
}

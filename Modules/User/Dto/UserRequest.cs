namespace BackEnd.Modules.User.Dto
{
    public class UpdateUserRequest
    {
        public required string FirstName { get; set; }
        public required string LastName { get; set; }
        public required string Gender { get; set; }
        public required string Role { get; set; }
        public required string Image { get; set; }
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

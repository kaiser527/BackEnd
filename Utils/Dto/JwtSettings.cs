namespace BackEnd.Utils.Dto
{
    public class JwtSettings
    {
        public string? Key { get; set; }
        public required string ValidIssuer { get; set; }
        public required string ValidAudience { get; set; }
        public required double Expires { get; set; }
    }
}

using BackEnd.Domain.Entities;

namespace BackEnd.Service.Interfaces
{
    public interface ITokenService
    {
        Task<string> GenerateToken(ApplicationUser user);
        string GenerateRefreshToken();
    }
}

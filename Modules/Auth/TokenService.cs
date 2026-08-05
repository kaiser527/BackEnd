using BackEnd.Modules.User.Entities;
using BackEnd.Utils.Dto;
using Microsoft.AspNetCore.Identity;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;

namespace BackEnd.Modules.Auth
{
    public class TokenService
    {
        private readonly SymmetricSecurityKey _secretKey;
        private readonly string? _validIssuer;
        private readonly string? _validAudience;
        private readonly double _expires;
        private readonly UserManager<ApplicationUser> _userManager;

        public TokenService(IConfiguration configuration, UserManager<ApplicationUser> userManager)
        {
            _userManager = userManager;

            var jwtSettings = configuration.GetSection("JwtSettings").Get<JwtSettings>();
            if (jwtSettings == null || string.IsNullOrEmpty(jwtSettings.Key))
            {
                throw new InvalidOperationException("JWT secret key is not configured.");
            }

            _secretKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtSettings.Key));
            _validIssuer = jwtSettings.ValidIssuer;
            _validAudience = jwtSettings.ValidAudience;
            _expires = jwtSettings.Expires;
        }

        public async Task<string> GenerateToken(ApplicationUser user)
        {
            var singingCredentials = new SigningCredentials(_secretKey, SecurityAlgorithms.HmacSha256);
            var claims = await GetClaimsAsync(user);
            var tokenOptions = GenerateTokenOptions(singingCredentials, claims);
            return new JwtSecurityTokenHandler().WriteToken(tokenOptions);
        }

        private async Task<List<Claim>> GetClaimsAsync(ApplicationUser user)
        {
            var claims = new List<Claim>
            {
                new (ClaimTypes.Name, user?.UserName ?? string.Empty),
                new (ClaimTypes.NameIdentifier, user?.Id ?? string.Empty),
                new (ClaimTypes.Email, user?.Email ?? string.Empty),
                new ("FirstName", user?.FirstName ?? string.Empty),
                new ("LastName", user?.LastName ?? string.Empty),
                new ("Gender", user?.Gender ?? string.Empty)
            };

            ArgumentNullException.ThrowIfNull(user);

            var roles = await _userManager.GetRolesAsync(user);
            claims.AddRange(roles.Select(role => new Claim(ClaimTypes.Role, role)));

            return claims;
        }

        private JwtSecurityToken GenerateTokenOptions(SigningCredentials signingCredentials, List<Claim> claims)
        {
            claims.Add(new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()));

            return new JwtSecurityToken(
                issuer: _validIssuer,
                audience: _validAudience,
                claims: claims,
                expires: DateTime.Now.AddMinutes(_expires),
                signingCredentials: signingCredentials
            );
        }

        public string GenerateRefreshToken()
        {
            var randomNumber = new byte[64];
            using var rng = RandomNumberGenerator.Create();
            rng.GetBytes(randomNumber);

            var refreshToken = Convert.ToBase64String(randomNumber);
            return refreshToken;
        }
    }
}

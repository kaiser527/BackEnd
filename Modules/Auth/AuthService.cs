using AutoMapper;
using BackEnd.Modules.Auth.Dto;
using BackEnd.Modules.Auth.Entities;
using BackEnd.Modules.Database;
using BackEnd.Modules.User;
using BackEnd.Modules.User.Dto;
using BackEnd.Modules.User.Entities;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;

namespace BackEnd.Modules.Auth
{
    public class AuthService(
        TokenService tokenService,
        UserManager<ApplicationUser> userManager,
        RoleManager<IdentityRole> roleManager,
        IMapper mapper,
        ILogger<UserService> logger,
        ApplicationDbContext context,
        IHttpContextAccessor httpContextAccessor
    )
    {
        private readonly IMapper _mapper = mapper;
        private readonly ILogger<UserService> _logger = logger;
        private readonly ApplicationDbContext _context = context;
        private readonly UserManager<ApplicationUser> _userManager = userManager;
        private readonly RoleManager<IdentityRole> _roleManager = roleManager;
        private readonly TokenService _tokenService = tokenService;
        private readonly IHttpContextAccessor _httpContextAccessor = httpContextAccessor;

        public async Task<UserResponse> LoginAsync(UserLoginRequest request)
        {
            if (request == null)
            {
                _logger.LogError("Login request is null");
                throw new ArgumentNullException(nameof(request));
            }

            // Fetch user including roles in a single query
            var userWithRoles = await _context.Users
                .Where(u => u.Email == request.Email)
                .Select(u => new
                {
                    User = u,
                    Roles = _context.UserRoles
                             .Where(ur => ur.UserId == u.Id)
                             .Join(_context.Roles,
                                   ur => ur.RoleId,
                                   r => r.Id,
                                   (ur, r) => r.Name)
                             .ToList()
                })
                .FirstOrDefaultAsync();

            if (userWithRoles == null || !await _userManager.CheckPasswordAsync(userWithRoles.User, request.Password))
            {
                _logger.LogError("Invalid email or password");
                throw new UnauthorizedAccessException("Invalid email or password");
            }

            var user = userWithRoles.User;
            var roles = userWithRoles.Roles;

            // Generate tokens
            var accessToken = await _tokenService.GenerateToken(user);
            var refreshToken = _tokenService.GenerateRefreshToken();
            var refreshTokenHash = SHA256.HashData(Encoding.UTF8.GetBytes(refreshToken));

            user.RefreshToken = Convert.ToBase64String(refreshTokenHash);
            user.RefreshTokenExpiryTime = DateTime.Now.AddDays(2);

            // Update user with refresh token
            var result = await _userManager.UpdateAsync(user);
            if (!result.Succeeded)
            {
                var errors = string.Join(", ", result.Errors.Select(e => e.Description));
                _logger.LogError("Failed to update user: {Errors}", errors);
                throw new Exception($"Failed to update user: {errors}");
            }

            // Map to response
            var userResponse = _mapper.Map<UserResponse>(user);
            userResponse.Role = roles.FirstOrDefault() ?? "User";
            userResponse.AccessToken = accessToken;
            userResponse.RefreshToken = refreshToken;

            return userResponse;
        }

        public async Task<CurrentUserResponse> RefreshTokenAsync(RefreshTokenRequest request)
        {
            _logger.LogInformation("Refresh token");

            // Hash the incoming RefreshToken
            var refreshTokenHash = SHA256.HashData(Encoding.UTF8.GetBytes(request.RefreshToken));
            var hashedRefreshToken = Convert.ToBase64String(refreshTokenHash);

            // Find user including roles in one query
            var userWithRoles = await _context.Users
                .Where(u => u.RefreshToken == hashedRefreshToken)
                .Select(u => new
                {
                    User = u,
                    Roles = _context.UserRoles
                        .Where(ur => ur.UserId == u.Id)
                        .Join(_context.Roles,
                              ur => ur.RoleId,
                              r => r.Id,
                              (ur, r) => r.Name)
                        .ToList()
                })
                .FirstOrDefaultAsync();

            if (userWithRoles == null)
            {
                _logger.LogError("Invalid refresh token");
                throw new UnauthorizedAccessException("Invalid refresh token");
            }

            var user = userWithRoles.User;
            var roles = userWithRoles.Roles;

            // Validate refresh token expiry
            if (user.RefreshTokenExpiryTime < DateTime.Now)
            {
                _logger.LogWarning("Refresh token expired for user ID: {UserId}", user.Id);
                throw new UnauthorizedAccessException("Refresh token expired");
            }

            // Generate a new access token
            var newAccessToken = await _tokenService.GenerateToken(user);

            _logger.LogInformation("Access token generated successfully");

            var currentUserResponse = _mapper.Map<CurrentUserResponse>(user);
            currentUserResponse.Role = roles.FirstOrDefault() ?? "User";
            currentUserResponse.AccessToken = newAccessToken;

            return currentUserResponse;
        }

        public async Task<CurrentUserResponse> GetCurrentUserAsync()
        {
            var userId = _httpContextAccessor.HttpContext?.User?.FindFirstValue(ClaimTypes.NameIdentifier);

            ArgumentNullException.ThrowIfNull(userId, nameof(userId));

            var userWithRoles = await _context.Users
                .Where(u => u.Id == userId)
                .Select(u => new
                {
                    User = u,
                    Roles = _context.UserRoles
                                .Where(ur => ur.UserId == u.Id)
                                .Join(_context.Roles,
                                      ur => ur.RoleId,
                                      r => r.Id,
                                      (ur, r) => r.Name)
                                .ToList()
                })
                .FirstOrDefaultAsync();

            if (userWithRoles == null)
            {
                _logger.LogError("User not found");
                throw new Exception("User not found");
            }

            var userResponse = _mapper.Map<CurrentUserResponse>(userWithRoles.User);
            userResponse.Role = userWithRoles.Roles.FirstOrDefault() ?? "User";

            return userResponse;
        }

        public async Task<RevokeRefreshTokenResponse> RevokeRefreshToken(RefreshTokenRequest refreshTokenRemoveRequest)
        {
            _logger.LogInformation("Revoking refresh token");

            // Hash refresh token
            var refreshTokenHash = SHA256.HashData(Encoding.UTF8.GetBytes(refreshTokenRemoveRequest.RefreshToken));
            var hashedRefreshToken = Convert.ToBase64String(refreshTokenHash);

            // Find user based on refresh token
            var user = await _userManager.Users.FirstOrDefaultAsync(u => u.RefreshToken == hashedRefreshToken);
            if (user == null)
            {
                _logger.LogError("Invalid refresh token");
                throw new Exception("Invalid refresh token");
            }

            // Blacklist access token from header if available
            var httpContext = _httpContextAccessor.HttpContext;
            var authHeader = httpContext?.Request.Headers.Authorization.FirstOrDefault();

            if (!string.IsNullOrEmpty(authHeader) && authHeader.StartsWith("Bearer "))
            {
                var accessToken = authHeader["Bearer ".Length..].Trim();
                var handler = new JwtSecurityTokenHandler();
                var jwtToken = handler.ReadJwtToken(accessToken);

                var jti = jwtToken.Claims.FirstOrDefault(c => c.Type == JwtRegisteredClaimNames.Jti)?.Value;
                if (!string.IsNullOrEmpty(jti))
                {
                    var expiryTime = jwtToken.ValidTo;

                    var blacklistToken = new BlacklistToken
                    {
                        Jti = jti,
                        ExpiryTime = expiryTime
                    };

                    await _context.BlacklistTokens.AddAsync(blacklistToken);
                    await _context.SaveChangesAsync();

                    _logger.LogInformation("Previous access token blacklisted for user ID: {UserId}", user.Id);
                }
            }

            // Validate refresh token expiry
            if (user.RefreshTokenExpiryTime < DateTime.Now)
            {
                _logger.LogWarning("Refresh token expired for user ID: {UserId}", user.Id);
                throw new Exception("Refresh token expired");
            }

            // Remove refresh token
            user.RefreshToken = null;
            user.RefreshTokenExpiryTime = null;

            var result = await _userManager.UpdateAsync(user);
            if (!result.Succeeded)
            {
                _logger.LogError("Failed to update user");
                return new RevokeRefreshTokenResponse
                {
                    Message = "Failed to revoke refresh token"
                };
            }

            _logger.LogInformation("Refresh token revoked successfully");

            return new RevokeRefreshTokenResponse
            {
                Message = "Refresh token revoked successfully"
            };
        }

        public async Task<UserResponse> RegisterAsync(UserRegisterRequest request)
        {
            _logger.LogInformation("Registering user");
            var existingUser = await _userManager.FindByEmailAsync(request.Email);
            if (existingUser != null)
            {
                _logger.LogError("Email is already exists");
                throw new Exception("Email is already exists");
            }

            var newUser = _mapper.Map<ApplicationUser>(request);

            //Generate unique username
            newUser.UserName = GenerateUserName(request.FirstName, request.LastName);
            var result = await _userManager.CreateAsync(newUser, request.Password);
            if (!result.Succeeded)
            {
                var errors = string.Join(", ", result.Errors.Select(e => e.Description));
                _logger.LogError("Failed to create user: {errors}", errors);
                throw new Exception($"Failed to create user: {errors}");
            }

            _logger.LogInformation("User created successfully");

            newUser.CreatedAt = DateTime.Now;
            newUser.UpdatedAt = DateTime.Now;

            var roleResult = await _userManager.AddToRoleAsync(newUser, "User");
            if (!roleResult.Succeeded)
            {
                var errors = string.Join(", ", roleResult.Errors.Select(e => e.Description));
                _logger.LogError("Failed to assign role: {errors}", errors);
                throw new Exception($"Failed to assign role: {errors}");
            }

            _logger.LogInformation("Role assigned successfully");

            var userResponse = _mapper.Map<UserResponse>(newUser);
            var roles = await _userManager.GetRolesAsync(newUser);

            userResponse.Role = roles.FirstOrDefault() ?? "User";

            return userResponse;
        }

        private string GenerateUserName(string firstName, string lastName)
        {
            var baseUsername = $"{firstName}{lastName}".ToLower();

            // Check if the username already exists
            var username = baseUsername;
            var count = 1;
            while (_userManager.Users.Any(u => u.UserName == username))
            {
                username = $"{baseUsername}{count}";
                count++;
            }
            return username;
        }
    }
}

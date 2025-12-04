using AutoMapper;
using BackEnd.Domain.Contracts;
using BackEnd.Domain.Entities;
using BackEnd.Domain.Interfaces;
using BackEnd.Infrastructure.Context;
using BackEnd.Service.Interfaces;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Cryptography;
using System.Text;

namespace BackEnd.Service.Implementations
{
    public class UserServiceImpl(
        ITokenService tokenService,
        ICurrentUserService currentUserService,
        UserManager<ApplicationUser> userManager,
        RoleManager<IdentityRole> roleManager,
        IMapper mapper,
        ILogger<UserServiceImpl> logger,
        ApplicationDbContext context,               
        IHttpContextAccessor httpContextAccessor    
    ) : IUserService
    {
        private readonly ITokenService _tokenService = tokenService;
        private readonly ICurrentUserService _currentUserService = currentUserService;
        private readonly UserManager<ApplicationUser> _userManager = userManager;
        private readonly RoleManager<IdentityRole> _roleManager = roleManager;
        private readonly IMapper _mapper = mapper;
        private readonly ILogger<UserServiceImpl> _logger = logger;
        private readonly ApplicationDbContext _context = context;
        private readonly IHttpContextAccessor _httpContextAccessor = httpContextAccessor; 

        public async Task DeleteAsync(Guid id)
        {
            var userWithRoles = await _context.Users
                .Where(u => u.Id == id.ToString())
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

            if(userWithRoles.Roles.Any(r => r == "Admin"))
            {
                _logger.LogError("Cannot delete Admin User");
                throw new Exception("Cannot delete Admin User");
            }

            await _userManager.DeleteAsync(userWithRoles.User);
        }

        public async Task<UserResponse> GetByIdAsync(Guid id)
        {
            _logger.LogInformation("Find user by id");

            var userWithRoles = await _context.Users
                .Where(u => u.Id == id.ToString())
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

            var userResponse = _mapper.Map<UserResponse>(userWithRoles.User);
            userResponse.Role = userWithRoles.Roles.FirstOrDefault() ?? "User";

            return userResponse;
        }

        public async Task<CurrentUserResponse> GetCurrentUserAsync()
        {
            var userId = _currentUserService.GetUserId();

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
                throw new Exception("Invalid refresh token");
            }

            var user = userWithRoles.User;
            var roles = userWithRoles.Roles;

            // Validate refresh token expiry
            if (user.RefreshTokenExpiryTime < DateTime.Now)
            {
                _logger.LogWarning("Refresh token expired for user ID: {UserId}", user.Id);
                throw new Exception("Refresh token expired");
            }

            // Generate a new access token
            var newAccessToken = await _tokenService.GenerateToken(user);

            _logger.LogInformation("Access token generated successfully");

            var currentUserResponse = _mapper.Map<CurrentUserResponse>(user);
            currentUserResponse.Role = roles.FirstOrDefault() ?? "User";
            currentUserResponse.AccessToken = newAccessToken;

            return currentUserResponse;
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

        public async Task<UserResponse> UpdateAsync(Guid id, UpdateUserRequest request)
        {
            // Fetch user with roles in one query
            var userWithRoles = await _context.Users
                .Where(u => u.Id == id.ToString())
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

            var user = userWithRoles.User;
            var currentRoles = userWithRoles.Roles;
            var currentRole = currentRoles.FirstOrDefault();

            // Update user fields
            user.FirstName = request.FirstName;
            user.LastName = request.LastName;
            user.Email = request.Email;
            user.Gender = request.Gender;

            await _userManager.UpdateAsync(user); 

            if (!string.IsNullOrWhiteSpace(request.Role) && !string.Equals(currentRole, request.Role, StringComparison.OrdinalIgnoreCase))
            {
                if (!await _roleManager.RoleExistsAsync(request.Role))
                    throw new Exception($"Role '{request.Role}' does not exist.");

                if (currentRoles.Count != 0)
                    await _userManager.RemoveFromRolesAsync(user, currentRoles);

                await _userManager.AddToRoleAsync(user, request.Role);
                currentRole = request.Role; // Update the role after changes
            }

            var response = _mapper.Map<UserResponse>(user);
            response.Role = currentRole ?? "User";

            return response;
        }

        public async Task<PaginateReponse<UserResponse>> FetchUserPaginate(
            UserFilterRequest request,
            int pageNumber = 1,
            int pageSize = 10
        )
        {
            // Start with all users
            var query = _context.Users.AsQueryable();

            // Apply filters
            if (!string.IsNullOrWhiteSpace(request.Email))
                query = query.Where(u => (u.Email ?? "").Contains(request.Email));

            if (!string.IsNullOrWhiteSpace(request.FirstName))
                query = query.Where(u => u.FirstName.Contains(request.FirstName));

            if (!string.IsNullOrWhiteSpace(request.LastName))
                query = query.Where(u => u.LastName.Contains(request.LastName));

            if (request.CreatedAtRange != null && request.CreatedAtRange.Count == 2)
            {
                var start = request.CreatedAtRange[0];
                var end = request.CreatedAtRange[1];
                query = query.Where(u => u.CreatedAt >= start && u.CreatedAt <= end);
            }

            if (request.UpdatedAtRange != null && request.UpdatedAtRange.Count == 2)
            {
                var start = request.UpdatedAtRange[0];
                var end = request.UpdatedAtRange[1];
                query = query.Where(u => u.UpdatedAt >= start && u.UpdatedAt <= end);
            }

            // Filter by role name
            if (!string.IsNullOrWhiteSpace(request.Role))
            {
                query = query
                    .Join(
                        _context.UserRoles,
                        u => u.Id,
                        ur => ur.UserId,
                        (u, ur) => new { User = u, UserRole = ur }
                    )
                    .Join(
                        _context.Roles,
                        uu => uu.UserRole.RoleId,
                        r => r.Id,
                        (uu, r) => new { uu.User, RoleName = r.Name }
                    )
                    .Where(x => x.RoleName == request.Role)
                    .Select(x => x.User);
            }

            // Sorting
            if (request.SortByCreatedAt.HasValue)
                query = request.SortByCreatedAt.Value
                    ? query.OrderBy(u => u.CreatedAt)
                    : query.OrderByDescending(u => u.CreatedAt);

            if (request.SortByUpdatedAt.HasValue)
                query = request.SortByUpdatedAt.Value
                    ? query.OrderBy(u => u.UpdatedAt)
                    : query.OrderByDescending(u => u.UpdatedAt);

            // Pagination
            var totalUsers = await query.CountAsync();
            var totalPages = (int)Math.Ceiling(totalUsers / (double)pageSize);

            var users = await query
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            // Fetch roles for all users in a single query
            var userIds = users.Select(u => u.Id).ToList();
            var userRoles = await _context.UserRoles
                .Where(ur => userIds.Contains(ur.UserId))
                .Join(_context.Roles,
                      ur => ur.RoleId,
                      r => r.Id,
                      (ur, r) => new { ur.UserId, r.Name })
                .ToListAsync();

            // Map to UserResponse
            var userResponses = users.Select(u =>
            {
                var response = _mapper.Map<UserResponse>(u);
                response.Role = userRoles.FirstOrDefault(ur => ur.UserId == u.Id)?.Name ?? "User";
                return response;
            }).ToList();

            return new PaginateReponse<UserResponse>
            {
                PageNumber = pageNumber,
                PageSize = pageSize,
                TotalPages = totalPages,
                Data = userResponses
            };
        }
    }
}

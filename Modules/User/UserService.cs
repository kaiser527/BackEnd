using AutoMapper;

using BackEnd.Modules.Database;
using BackEnd.Modules.User.Dto;
using BackEnd.Modules.User.Entities;
using BackEnd.Utils.Dto;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace BackEnd.Modules.User
{
    public class UserService(
        UserManager<ApplicationUser> userManager,
        RoleManager<IdentityRole> roleManager,
        IMapper mapper,
        ILogger<UserService> logger,
        ApplicationDbContext context          
    )
    {
        private readonly UserManager<ApplicationUser> _userManager = userManager;
        private readonly RoleManager<IdentityRole> _roleManager = roleManager;
        private readonly IMapper _mapper = mapper;
        private readonly ILogger<UserService> _logger = logger;
        private readonly ApplicationDbContext _context = context;

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
            user.Gender = request.Gender;

            await _userManager.UpdateAsync(user); 

            if (!string.IsNullOrWhiteSpace(request.Role) && !string.Equals(currentRole, request.Role, StringComparison.OrdinalIgnoreCase))
            {
                if (!await _roleManager.RoleExistsAsync(request.Role))
                    throw new Exception($"Role '{request.Role}' does not exist.");

                if (currentRoles.Count != 0)
                    await _userManager.RemoveFromRolesAsync(user, currentRoles);

                await _userManager.AddToRoleAsync(user, request.Role);
                currentRole = request.Role; 
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

            var meta = new Meta
            {
               PageSize = pageSize,
               PageNumber = pageNumber,
               TotalPages = totalPages,
            };

            return new PaginateReponse<UserResponse>
            {
                Meta = meta,
                Data = userResponses
            };
        }
    }
}

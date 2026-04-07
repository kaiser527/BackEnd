using AutoMapper;
using BackEnd.Domain.Contracts;
using BackEnd.Service.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace BackEnd.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class UserController(IUserService userService, IMapper mapper) : Controller
    {
        private readonly IUserService _userService = userService;
        private readonly IMapper _mapper = mapper;

        [HttpGet("{id}")]
        [Authorize(Roles = "Admin,Manager")]
        public async Task<IActionResult> GetById(Guid id)
        {
            try
            {
                var response = await _userService.GetByIdAsync(id);
                var userResponse = _mapper.Map<UserResponse>(response);

                var apiResponse = new ApiResponse<UserResponse>
                {
                    StatusCode = 200,
                    Message = "Get user by Id successfully",
                    Result = userResponse
                };

                return Ok(apiResponse);
            }
            catch(BadHttpRequestException ex)
            {
                var errorResponse = _mapper.Map<ErrorResponse>(ex);
                return StatusCode(errorResponse.StatusCode, errorResponse);
            }
        }

        [HttpDelete("{id}")]
        [Authorize(Roles = "Admin,Manager")]
        public async Task<IActionResult> Delete(Guid id)
        {
            try
            {
                await _userService.DeleteAsync(id);

                return Ok(new { Status = 200, Message = "Delete User successfully" });
            }
            catch(BadHttpRequestException ex)
            {
                var errorResponse = _mapper.Map<ErrorResponse>(ex);
                return StatusCode(errorResponse.StatusCode, errorResponse);
            }
        }

        [HttpPatch("{id}")]
        [Authorize(Roles = "Admin,Manager")]
        public async Task<IActionResult> Update(Guid id, [FromBody] UpdateUserRequest request)
        {
            try
            {
                var response = await _userService.UpdateAsync(id, request);
                var userResponse = _mapper.Map<UserResponse>(response);

                var apiResponse = new ApiResponse<UserResponse>
                {
                    StatusCode = 200,
                    Message = "Update user successfully",
                    Result = userResponse
                };

                return Ok(apiResponse);
            }
            catch(BadHttpRequestException ex)
            {
                var errorResponse = _mapper.Map<ErrorResponse>(ex);
                return StatusCode(errorResponse.StatusCode, errorResponse);
            }
        }

        [HttpGet]
        [Authorize(Roles = "Admin,Manager")]
        public async Task<IActionResult> GetUsers([FromQuery] int pageNumber = 1, [FromQuery] int pageSize = 10)
        {
            try
            {
                var filter = new UserFilterRequest
                {
                    Email = Request.Query["email"],
                    FirstName = Request.Query["firstName"],
                    LastName = Request.Query["lastName"],
                    Role = Request.Query["role"]
                };

                if (!string.IsNullOrWhiteSpace(Request.Query["createdAtRange"]))
                {
                    var dates = Request.Query["createdAtRange"].ToString().Split(',');
                    if (dates.Length == 2 &&
                        DateTime.TryParse(dates[0], out var startDate) &&
                        DateTime.TryParse(dates[1], out var endDate))
                    {
                        filter.CreatedAtRange = [startDate, endDate];
                    }
                }

                if (!string.IsNullOrWhiteSpace(Request.Query["updatedAtRange"]))
                {
                    var dates = Request.Query["updatedAtRange"].ToString().Split(',');
                    if (dates.Length == 2 &&
                        DateTime.TryParse(dates[0], out var startDate) &&
                        DateTime.TryParse(dates[1], out var endDate))
                    {
                        filter.UpdatedAtRange = [startDate, endDate];
                    }
                }

                if (bool.TryParse(Request.Query["sortByCreatedAt"], out var sortCreated))
                    filter.SortByCreatedAt = sortCreated;

                if (bool.TryParse(Request.Query["sortByUpdatedAt"], out var sortUpdated))
                    filter.SortByUpdatedAt = sortUpdated;

                var response = await _userService.FetchUserPaginate(filter, pageNumber, pageSize);

                return Ok(new ApiResponse<PaginateReponse<UserResponse>>
                {
                    StatusCode = 200,
                    Message = "Users fetched successfully",
                    Result = response
                });
            }
            catch (BadHttpRequestException ex)
            {
                var errorResponse = _mapper.Map<ErrorResponse>(ex);
                return StatusCode(errorResponse.StatusCode, errorResponse);
            }
        }
    }
}


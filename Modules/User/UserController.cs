using AutoMapper;
using BackEnd.Modules.User.Dto;
using BackEnd.Utils.Dto;
using BackEnd.Utils.Helper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace BackEnd.Modules.User
{
    [ApiController]
    [Route("api/[controller]")]
    public class UserController(UserService userService, IMapper mapper) : Controller
    {
        private readonly UserService _userService = userService;
        private readonly IMapper _mapper = mapper;

        [HttpGet("{id}")]
        [Authorize(Roles = "Admin,Staff")]
        public async Task<IActionResult> GetById(Guid id)
        {
            return await ExceptionWrapper.Execute(async () =>
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
            }, _mapper);
        }

        [HttpDelete("{id}")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Delete(Guid id)
        {
           return await ExceptionWrapper.Execute(async () =>
           {
               await _userService.DeleteAsync(id);
               return Ok(new { Status = 200, Message = "Delete User successfully" });
           }, _mapper);
        }

        [HttpPatch("{id}")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Update(Guid id, [FromBody] UpdateUserRequest request)
        {
            return await ExceptionWrapper.Execute(async () =>
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
            }, _mapper);
        }

        [HttpGet]
        [Authorize(Roles = "Admin,Staff")]
        public async Task<IActionResult> GetUsers([FromQuery] int pageNumber = 1, [FromQuery] int pageSize = 10)
        {
            return await ExceptionWrapper.Execute(async () =>
            {
                var filter = new UserFilterRequest
                {
                    Email = Request.Query["email"],
                    FirstName = Request.Query["firstName"],
                    LastName = Request.Query["lastName"],
                    Role = Request.Query["role"]
                };
                QsDateFilter.NormalizeDateFilter(Request, filter);
                var response = await _userService.FetchUserPaginate(filter, pageNumber, pageSize);
                return Ok(new ApiResponse<PaginateReponse<UserResponse>>
                {
                    StatusCode = 200,
                    Message = "Users fetched successfully",
                    Result = response
                });
            }, _mapper);
        }
    }
}


using AutoMapper;
using BackEnd.Domain.Contracts;
using BackEnd.Service.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace BackEnd.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AuthController(IUserService userService, IMapper mapper) : ControllerBase
    {
        private readonly IUserService _userService = userService;
        private readonly IMapper _mapper = mapper;

        [HttpPost("register")]
        [AllowAnonymous]
        public async Task<IActionResult> Register([FromBody] UserRegisterRequest request)
        {
            try
            {
                var user = await _userService.RegisterAsync(request);

                var userResponse = _mapper.Map<UserResponse>(user);

                var apiResponse = new ApiResponse<UserResponse>
                {
                    StatusCode = 200,
                    Message = "User registered successfully",
                    Result = userResponse
                };

                return Ok(apiResponse);
            }
            catch (Exception ex)
            {
                var errorResponse = _mapper.Map<ErrorResponse>(ex);
                return StatusCode(errorResponse.StatusCode, errorResponse);
            }
        }

        [HttpPost("login")]
        [AllowAnonymous]
        public async Task<IActionResult> Login([FromBody] UserLoginRequest request)
        {
            try
            {
                var user = await _userService.LoginAsync(request);

                var userResponse = _mapper.Map<UserResponse>(user);

                var apiResponse = new ApiResponse<UserResponse>
                {
                    StatusCode = 200,
                    Message = "User login successfully",
                    Result = userResponse
                };

                return Ok(apiResponse);
            }
            catch(Exception ex)
            {
                var errorResponse = _mapper.Map<ErrorResponse>(ex);
                return StatusCode(errorResponse.StatusCode, errorResponse);
            }
        }

        [HttpPost("refresh-token")]
        [AllowAnonymous]
        public async Task<IActionResult> RefreshToken([FromBody] RefreshTokenRequest request)
        {
            try
            {
                var currentUser = await _userService.RefreshTokenAsync(request);
                var userResponse = _mapper.Map<CurrentUserResponse>(currentUser);

                var apiResponse = new ApiResponse<CurrentUserResponse>
                {
                    StatusCode = 200,
                    Message = "Refresh token successfully",
                    Result = userResponse
                };

                return Ok(apiResponse);
            }
            catch(Exception ex)
            {
                var errorResponse = _mapper.Map<ErrorResponse>(ex);
                return StatusCode(errorResponse.StatusCode, errorResponse);
            }
        }

        [HttpPost("revoke-refresh-token")]
        [Authorize]
        public async Task<IActionResult> RevokeRefreshToken([FromBody] RefreshTokenRequest request)
        {
            try
            {
                var response = await _userService.RevokeRefreshToken(request);

                var apiResponse = new ApiResponse<RevokeRefreshTokenResponse>
                {
                    StatusCode = response.Message == "Refresh token revoked successfully" ? 200 : 400,
                    Message = response.Message,
                    Result = response
                };

                return response.Message == "Refresh token revoked successfully"
                    ? Ok(apiResponse)
                    : BadRequest(apiResponse);
            }
            catch (Exception ex)
            {
                var errorResponse = _mapper.Map<ErrorResponse>(ex);
                return StatusCode(errorResponse.StatusCode, errorResponse);
            }
        }

        [HttpGet("current-user")]
        [Authorize]
        public async Task<IActionResult> GetCurrentUser()
        {
            try
            {
                var response = await _userService.GetCurrentUserAsync();

                var apiResponse = new ApiResponse<CurrentUserResponse>
                {
                    StatusCode = 200,
                    Message = "Get current user successfully",
                    Result = response
                };

                return Ok(apiResponse);
            }
            catch (Exception ex)
            {
                var errorResponse = _mapper.Map<ErrorResponse>(ex);
                return StatusCode(errorResponse.StatusCode, errorResponse);
            }
        }
    }
}

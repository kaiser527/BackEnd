using AutoMapper;
using BackEnd.Modules.Auth.Dto;
using BackEnd.Modules.User.Dto;
using BackEnd.Utils.Dto;
using BackEnd.Utils.Helper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace BackEnd.Modules.Auth
{
    [ApiController]
    [Route("api/[controller]")]
    public class AuthController(AuthService authService, IMapper mapper) : ControllerBase
    {
        private readonly AuthService _authService = authService;
        private readonly IMapper _mapper = mapper;

        [HttpPost("register")]
        [AllowAnonymous]
        public async Task<IActionResult> Register([FromBody] UserRegisterRequest request)
        {
            return await ExceptionWrapper.Execute(async () =>
            {
                var user = await _authService.RegisterAsync(request);
                var userResponse = _mapper.Map<UserResponse>(user);
                var apiResponse = new ApiResponse<UserResponse>
                {
                    StatusCode = 201,
                    Message = "User registered successfully",
                    Result = userResponse
                };
                return Ok(apiResponse);
            }, _mapper);
        }

        [HttpPost("login")]
        [AllowAnonymous]
        public async Task<IActionResult> Login([FromBody] UserLoginRequest request)
        {
            return await ExceptionWrapper.Execute(async () =>
            {
                var user = await _authService.LoginAsync(request);
                var userResponse = _mapper.Map<UserResponse>(user);
                var apiResponse = new ApiResponse<UserResponse>
                {
                    StatusCode = 200,
                    Message = "User login successfully",
                    Result = userResponse
                };
                return Ok(apiResponse);
            }, _mapper);
        }

        [HttpPost("refresh-token")]
        [AllowAnonymous]
        public async Task<IActionResult> RefreshToken([FromBody] RefreshTokenRequest request)
        {
           return await ExceptionWrapper.Execute(async () =>
           {
                var currentUser = await _authService.RefreshTokenAsync(request);
                var userResponse = _mapper.Map<CurrentUserResponse>(currentUser);
                var apiResponse = new ApiResponse<CurrentUserResponse>
                {
                    StatusCode = 200,
                    Message = "Refresh token successfully",
                    Result = userResponse
                };
                return Ok(apiResponse);
           }, _mapper);
        }

        [HttpPost("revoke-refresh-token")]
        [Authorize]
        public async Task<IActionResult> RevokeRefreshToken([FromBody] RefreshTokenRequest request)
        {
            return await ExceptionWrapper.Execute(async () =>
            {
                var response = await _authService.RevokeRefreshToken(request);
                var apiResponse = new ApiResponse<RevokeRefreshTokenResponse>
                {
                    StatusCode = response.Message == "Refresh token revoked successfully" ? 200 : 400,
                    Message = response.Message,
                    Result = response
                };
                return response.Message == "Refresh token revoked successfully"
                    ? Ok(apiResponse)
                    : BadRequest(apiResponse);
            }, _mapper);    
        }

        [HttpGet("current-user")]
        [Authorize]
        public async Task<IActionResult> GetCurrentUser()
        {
            return await ExceptionWrapper.Execute(async () =>
            {
                var response = await _authService.GetCurrentUserAsync();
                var apiResponse = new ApiResponse<CurrentUserResponse>
                {
                    StatusCode = 200,
                    Message = "Get current user successfully",
                    Result = response
                };
                return Ok(apiResponse);
            }, _mapper);
        }
    }
}

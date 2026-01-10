using BuildingBlocks.Auth.Extensions;
using BuildingBlocks.Middleware.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using User.Application.DTOs;
using User.Application.Services;

namespace User.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class UsersController : ControllerBase
    {
        private readonly IUserService _userService;
        private readonly ILogger<UsersController> _logger;

        public UsersController(IUserService userService, ILogger<UsersController> logger)
        {
            _userService = userService;
            _logger = logger;
        }

        [HttpGet]
        [Authorize(Roles = "admin,manager")]
        public async Task<ActionResult<ApiResponse<List<UserDto>>>> GetAllUsers()
        {
            _logger.LogInformation("User {Email} fetching all users", User.GetUserEmail());
            var users = await _userService.GetAllUsersAsync();
            return Ok(ApiResponse<List<UserDto>>.SuccessResponse(users, "Users retrieved successfully"));
        }

        [HttpGet("{id}")]
        [Authorize(Roles = "admin,manager")]
        public async Task<ActionResult<ApiResponse<UserDto>>> GetUserById(Guid id)
        {
            var user = await _userService.GetUserByIdAsync(id);
            if (user == null)
            {
                return NotFound(ApiResponse<UserDto>.ErrorResponse("User not found"));
            }

            return Ok(ApiResponse<UserDto>.SuccessResponse(user));
        }

        [HttpGet("profile")]
        public async Task<ActionResult<ApiResponse<UserDto>>> GetMyProfile()
        {
            var email = User.GetUserEmail();
            if (string.IsNullOrEmpty(email))
            {
                return Unauthorized(ApiResponse<UserDto>.ErrorResponse("User email not found"));
            }

            var user = await _userService.GetUserByEmailAsync(email);
            if (user == null)
            {
                return NotFound(ApiResponse<UserDto>.ErrorResponse("User profile not found"));
            }

            return Ok(ApiResponse<UserDto>.SuccessResponse(user));
        }

        [HttpPost]
        [Authorize(Roles = "admin")]
        public async Task<ActionResult<ApiResponse<UserDto>>> CreateUser([FromBody] CreateUserDto createUserDto)
        {
            _logger.LogInformation("Creating new user with email {Email}", createUserDto.Email);
            var user = await _userService.CreateUserAsync(createUserDto);
            return CreatedAtAction(
                nameof(GetUserById),
                new { id = user.Id },
                ApiResponse<UserDto>.SuccessResponse(user, "User created successfully")
            );
        }

        [HttpPut("{id}")]
        [Authorize(Roles = "admin")]
        public async Task<ActionResult<ApiResponse<UserDto>>> UpdateUser(
            Guid id,
            [FromBody] UpdateUserDto updateUserDto)
        {
            var user = await _userService.UpdateUserAsync(id, updateUserDto);
            if (user == null)
            {
                return NotFound(ApiResponse<UserDto>.ErrorResponse("User not found"));
            }

            return Ok(ApiResponse<UserDto>.SuccessResponse(user, "User updated successfully"));
        }

        [HttpDelete("{id}")]
        [Authorize(Roles = "admin")]
        public async Task<ActionResult<ApiResponse<object>>> DeleteUser(Guid id)
        {
            var result = await _userService.DeleteUserAsync(id);
            if (!result)
            {
                return NotFound(ApiResponse<object>.ErrorResponse("User not found"));
            }

            return Ok(ApiResponse<object>.SuccessResponse(null!, "User deleted successfully"));
        }
    }
}
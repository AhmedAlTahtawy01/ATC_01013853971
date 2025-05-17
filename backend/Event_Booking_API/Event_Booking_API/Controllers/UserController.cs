using Microsoft.AspNetCore.Mvc;
using BusinessLogic.Services;
using DataAccess.Models;
using EventBooking.RequestModels;
using Microsoft.Extensions.Logging;
using Microsoft.AspNetCore.Authorization;

namespace EventBooking.Controllers
{
    [Authorize]
    [Route("api/[controller]")]
    [ApiController]
    public class UserController : ControllerBase
    {
        private readonly UserService _userService;
        private readonly TokenService _tokenService;
        private readonly ILogger<UserController> _logger;

        public UserController(UserService userService, TokenService tokenService, ILogger<UserController> logger)
        {
            _userService = userService;
            _tokenService = tokenService;
            _logger = logger;
        }

        [AllowAnonymous]
        [HttpPost("register")]
        public async Task<IActionResult> Register([FromBody] User user)
        {
            if (!ModelState.IsValid)
            {
                _logger.LogError("Invalid user data");
                return BadRequest(new { message = "Invalid user data", errors = ModelState });
            }

            try
            {
                _logger.LogInformation("Registering user: {Username}", user.Username);
                int newUserId = await _userService.CreateUserAsync(user);

                if (newUserId > 0)
                {
                    _logger.LogInformation("User registered successfully: {Username}", user.Username);
                    return Ok(new { message = "User registered successfully", userId = newUserId });
                }

                _logger.LogError("Failed to register user: {Username}", user.Username);
                return BadRequest(new { message = "Failed to register user, username or email already exists" });
            }
            catch (ArgumentException ex)
            {
                _logger.LogError(ex, "Error registering user: {Username}", user.Username);
                return BadRequest(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error registering user: {Username}", user.Username);
                return StatusCode(500, new { message = "Internal server error" });
            }
        }

        [AllowAnonymous]
        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] LoginRequest request)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    _logger.LogError("Invalid login request");
                    return BadRequest(new { message = "Invalid login request", errors = ModelState });
                }

                var user = await _userService.LoginAsync(request.Username, request.Password);
                if (user == null)
                {
                    _logger.LogWarning("Login failed for user: {Username}", request.Username);
                    return Unauthorized(new { message = "Invalid username or password" });
                }
                var token = _tokenService.GenerateToken(user);

                _logger.LogInformation("Login successful for user: {Username}", request.Username);
                return Ok(new
                {
                    message = "Login successful",
                    token,
                    user = new
                    {
                        user.Id,
                        user.Username,
                        user.Name,
                        user.Email,
                        user.RoleId
                    }
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error logging in: {Username}", request.Username);
                return StatusCode(500, new { message = "Exception thrown", error = ex.Message });

            }
        }

        [Authorize(Roles = "1")]
        [HttpGet]
        public async Task<IActionResult> GetAllUsers([FromQuery] int pageNumber = 1, [FromQuery] int pageSize = 10)
        {
            try
            {
                _logger.LogInformation("Retrieving all users with pagination");
                var users = await _userService.GetAllUsersAsync(pageNumber, pageSize);
                if (users.Count == 0)
                {
                    _logger.LogWarning("No users found");
                    return NotFound(new { message = "No users found" });
                }

                var validatedUsers = users.Select(u => new
                {
                    u.Id,
                    u.Username,
                    u.Email,
                    u.RoleId,
                    PasswordHash = !string.IsNullOrEmpty(u.PasswordHash) ? "********" : null
                });
                return Ok(new { message = "Users retrieved successfully", users = validatedUsers });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving all users with pagination");
                return StatusCode(500, new { message = "Internal server error" });
            }
        }

        [Authorize(Roles = "1")]
        [HttpGet("count")]
        public async Task<IActionResult> GetUserCount()
        {
            try
            {
                _logger.LogInformation("Retrieving total user count");
                var count = await _userService.GetUserCountAsync();

                return Ok(new { message = "User count retrieved successfully", count });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving user count");
                return StatusCode(500, new { message = "Internal server error" });
            }
        }

        [HttpGet("{userId}")]
        public async Task<IActionResult> GetById([FromRoute] int userId)
        {
            try
            {
                _logger.LogInformation("Retrieving user by ID: {UserId}", userId);
                var user = await _userService.GetByIdAsync(userId);

                if (user == null)
                {
                    _logger.LogWarning("User not found with ID: {UserId}", userId);
                    return NotFound(new { message = "User not found" });
                }

                var validatedUser = new
                {
                    user.Id,
                    user.Username,
                    user.Email,
                    user.RoleId,
                    PasswordHash = !string.IsNullOrEmpty(user.PasswordHash) ? "********" : null
                };

                return Ok(new { message = "User retrieved successfully", user = validatedUser });
            }
            catch (KeyNotFoundException ex)
            {
                _logger.LogError(ex, "Error retrieving user by ID: {UserId}", userId);
                return NotFound(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving user by ID: {UserId}", userId);
                return StatusCode(500, new { message = "Internal server error" });
            }
        }

        [HttpGet("username/{username}")]
        public async Task<IActionResult> GetByUsername([FromRoute] string username)
        {
            try
            {
                _logger.LogInformation("Retrieving user by username: {Username}", username);
                var user = await _userService.GetByUsernameAsync(username);

                if (user == null)
                {
                    _logger.LogWarning("User not found with username: {Username}", username);
                    return NotFound(new { message = "User not found" });
                }

                var validatedUser = new
                {
                    user.Id,
                    user.Username,
                    user.Email,
                    user.RoleId,
                    PasswordHash = !string.IsNullOrEmpty(user.PasswordHash) ? "********" : null
                };

                return Ok(new { message = "User retrieved successfully", user = validatedUser });
            }
            catch (KeyNotFoundException ex)
            {
                _logger.LogError(ex, "Error retrieving user by username: {Username}", username);
                return NotFound(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving user by username: {Username}", username);
                return StatusCode(500, new { message = "Internal server error" });
            }
        }

        [HttpGet("email/{email}")]
        public async Task<IActionResult> GetByEmail([FromRoute] string email)
        {
            try
            {
                _logger.LogInformation("Retrieving user by email: {Email}", email);
                var user = await _userService.GetByEmailAsync(email);

                if (user == null)
                {
                    _logger.LogWarning("User not found with email: {Email}", email);
                    return NotFound(new { message = "User not found" });
                }

                var validatedUser = new
                {
                    user.Id,
                    user.Username,
                    user.Email,
                    user.RoleId,
                    PasswordHash = !string.IsNullOrEmpty(user.PasswordHash) ? "********" : null
                };

                return Ok(new { message = "User retrieved successfully", user = validatedUser });
            }
            catch (KeyNotFoundException ex)
            {
                _logger.LogError(ex, "Error retrieving user by email: {Email}", email);
                return NotFound(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving user by email: {Email}", email);
                return StatusCode(500, new { message = "Internal server error" });
            }
        }

        [HttpPut("{userId}")]
        public async Task<IActionResult> UpdateUser([FromRoute] int userId, [FromBody] User user)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    _logger.LogError("Invalid user data");
                    return BadRequest(new { message = "Invalid user data", errors = ModelState });
                }

                if (user.Id != userId)
                {
                    _logger.LogError("User ID in route does not match body: Route={UserId}, Body={BodyId}", userId, user.Id);
                    return BadRequest(new { message = "User ID mismatch." });
                }

                bool updated = await _userService.UpdateUserAsync(user);
                if (updated)
                {
                    _logger.LogInformation("User updated successfully: {UserId}", userId);
                    return Ok(new { message = "User updated successfully" });
                }

                _logger.LogError("Failed to update user: {UserId}", userId);
                return BadRequest(new { message = "Failed to update user" });
            }
            catch (ArgumentException ex)
            {
                _logger.LogError(ex, "Error updating user: {UserId}", userId);
                return BadRequest(new { message = ex.Message });
            }
            catch (InvalidOperationException ex)
            {
                _logger.LogError(ex, "Error updating user: {UserId}", userId);
                return NotFound(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating user: {UserId}", userId);
                return StatusCode(500, new { message = "Internal server error" });
            }
        }

        [HttpDelete("{userId}")]
        public async Task<IActionResult> DeleteUser([FromRoute] int userId)
        {
            try
            {
                _logger.LogInformation("Deleting user by ID: {UserId}", userId);
                bool deleted = await _userService.DeleteUserAsync(userId);

                if (deleted)
                {
                    _logger.LogInformation("User deleted successfully: {UserId}", userId);
                    return Ok(new { message = "User deleted successfully" });
                }

                _logger.LogError("Failed to delete user: {UserId}", userId);
                return BadRequest(new { message = "Failed to delete user" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting user: {UserId}", userId);
                return StatusCode(500, new { message = "Internal server error" });
            }
        }

        [Authorize(Roles = "1")]
        [HttpPost("change-role")]
        public async Task<IActionResult> ChangeUserRole([FromBody] ChangeRoleRequest request)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    _logger.LogError("Invalid change role request");
                    return BadRequest(new { message = "Invalid change role request", errors = ModelState });
                }

                bool changed = await _userService.ChangeUserRoleAsync(request.UserId, request.NewRoleId);
                if (changed)
                {
                    _logger.LogInformation("User role changed successfully: {UserId}", request.UserId);
                    return Ok(new { message = "User role changed successfully" });
                }

                _logger.LogError("Failed to change user role: {UserId}", request.UserId);
                return BadRequest(new { message = "Failed to change user role" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error changing user role: {UserId}", request.UserId);
                return StatusCode(500, new { message = "Internal server error" });
            }
        }
    }
}
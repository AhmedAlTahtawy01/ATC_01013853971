using Microsoft.AspNetCore.Mvc;
using ATC.BusinessLogic.Services;
using ATC.DataAccess.Models;
using ATC.EventBooking.RequestModels;
using Microsoft.AspNetCore.Authentication;
using Microsoft.Extensions.Logging;

[Route("api/[controller]")]
[ApiController]
public class UserController : ControllerBase
{
    private readonly UserService _userService;
    private readonly ILogger<UserController> _logger;

    public UserController(UserService userService, ILogger<UserController> logger)
    {
        _userService = userService;
        _logger = logger;
    }

    [HttpPost("register")]
    public async Task<IActionResult> Register([FromBody] User user)
    {
        if (!ModelState.IsValid)
        {
            _logger.LogError("Invalid user data");
            return BadRequest(ModelState);
        }

        bool created = await _userService.CreateUserAsync(user);
        if (!created)
        {
            _logger.LogError("Failed to register user: {Username}", user.Username);
            return BadRequest(new { message = "Failed to register user, username or email already exists" });
        }

        _logger.LogInformation("User registered successfully: {Username}", user.Username);
        return Ok(new { message = "User registered successfully" });
    }

    [HttpPost("login")]
    public async Task<IActionResult> Login([FromBody] LoginRequest request)
    {
        if (!ModelState.IsValid)
        {
            _logger.LogError("Invalid login request");
            return BadRequest(ModelState);
        }

        var user = await _userService.LoginAsync(request.Username, request.Password);

        if (user == null)
        {
            _logger.LogWarning("Invalid username or password");
            return Unauthorized(new { message = "Invalid username or password" });
        }

        _logger.LogInformation("Login successful for user: {Username}", request.Username);
        return Ok(new { message = "Login successful", user = user });
    }

    [HttpGet]
    public async Task<IActionResult> GetAllUsers([FromQuery] int pageNumber = 1, [FromQuery] int pageSize = 10)
    {
        var users = await _userService.GetAllUsersAsync(pageNumber, pageSize);
        return Ok(users);
    }

    [HttpGet("{userId}")]
    public async Task<IActionResult> GetById([FromRoute] int userId)
    {
        if (userId <= 0)
        {
            _logger.LogError("Invalid user ID: {UserId}", userId);
            return BadRequest(new { message = "Invalid user ID" });
        }

        var user = await _userService.GetByIdAsync(userId);
        if (user == null)
        {
            _logger.LogError("User not found: {UserId}", userId);
            return NotFound(new { message = "User not found" });
        }

        return Ok(user);
    }

    [HttpGet("username/{username}")]
    public async Task<IActionResult> GetByUsername([FromRoute] string username)
    {
        if (string.IsNullOrEmpty(username))
        {
            _logger.LogError("Invalid username");
            return BadRequest(new { message = "Invalid username" });
        }

        var user = await _userService.GetByUsernameAsync(username);
        if (user == null)
        {
            _logger.LogError("User not found: {Username}", username);
            return NotFound(new { message = "User not found" });
        }

        return Ok(user);
    }

    [HttpGet("email/{email}")]
    public async Task<IActionResult> GetByEmail([FromRoute] string email)
    {
        if (string.IsNullOrEmpty(email))
        {
            _logger.LogError("Invalid email");
            return BadRequest(new { message = "Invalid email" });
        }

        var user = await _userService.GetByEmailAsync(email);
        if (user == null)
        {
            _logger.LogError("User not found: {Email}", email);
            return NotFound(new { message = "User not found" });
        }

        return Ok(user);
    }

    [HttpPut("{userId}")]
    public async Task<IActionResult> UpdateUser([FromRoute] int userId, [FromBody] User user)
    {
        if (!ModelState.IsValid)
        {
            _logger.LogError("Invalid user data");
            return BadRequest(ModelState);
        }

        if (userId <= 0)
        {
            _logger.LogError("Invalid user ID: {UserId}", userId);
            return BadRequest(new { message = "Invalid user ID" });
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

    [HttpDelete("{userId}")]
    public async Task<IActionResult> DeleteUser([FromRoute] int userId)
    {
        if (userId <= 0)
        {
            _logger.LogError("Invalid user ID: {UserId}", userId);
            return BadRequest(new { message = "Invalid user ID" });
        }

        bool deleted = await _userService.DeleteUserAsync(userId);
        if (deleted)
        {
            _logger.LogInformation("User deleted successfully: {UserId}", userId);
            return Ok(new { message = "User deleted successfully" });
        }

        _logger.LogError("Failed to delete user: {UserId}", userId);
        return BadRequest(new { message = "Failed to delete user" });
    }

    [HttpPost("change-role")]
    public async Task<IActionResult> ChangeUserRole([FromBody] ChangeRoleRequest request)
    {
        if (!ModelState.IsValid)
        {
            _logger.LogError("Invalid change role request");
            return BadRequest(ModelState);
        }

        bool changed = await _userService.ChangeUserRoleAsync(request.UserId, request.NewRoleId);
        if (changed)
        {
            _logger.LogInformation("User role changed successfully: {UserId}", request.UserId);
            return Ok(new { message = "User role changed successfully" });
        }

        return BadRequest(new { message = "Failed to change user role" });
    }
}

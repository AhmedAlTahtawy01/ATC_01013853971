using Microsoft.AspNetCore.Mvc;
using ATC.BusinessLogic.Services;
using ATC.DataAccess.Models;
using ATC.EventBooking.RequestModels;
using Microsoft.AspNetCore.Authentication;
using Microsoft.Extensions.Logging;

[Route("api/[controller]")]
[ApiController]
public class RoleController : ControllerBase
{
    private readonly RoleService _roleService;
    private readonly ILogger<RoleController> _logger;

    public RoleController(RoleService roleService, ILogger<RoleController> logger)
    {
        _roleService = roleService;
        _logger = logger;
    }

    [HttpPost]
    public async Task<IActionResult> CreateRole([FromBody] Role role)
    {
        if (!ModelState.IsValid)
        {
            _logger.LogError("Invalid role data");
            return BadRequest(ModelState);
        }

        bool created = await _roleService.CreateRoleAsync(role);
        if (!created)
        {
            _logger.LogError("Failed to create role: {RoleName}", role.Name);
            return BadRequest(new { message = "Failed to create role, role name is not unique" });
        }

        _logger.LogInformation("Role created successfully: {RoleName}", role.Name);
        return Ok(new { message = "Role created successfully" });
    }

    [HttpGet]
    public async Task<IActionResult> GetAllRoles()
    {
        try
        {
            var roles = await _roleService.GetAllRolesAsync();
            return Ok(roles);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while retrieving all roles");
            return StatusCode(500, new { message = "Error retrieving roles" });
        }
    }

    [HttpGet("{roleId}")]
    public async Task<IActionResult> GetRoleById([FromRoute] int roleId)
    {
        if (roleId <= 0)
        {
            _logger.LogError("Invalid role ID: {RoleId}", roleId);
            return BadRequest(new { message = "Invalid role ID" });
        }

        var role = await _roleService.GetRoleByIdAsync(roleId);
        if (role == null)
        {
            _logger.LogError("Role not found: {RoleId}", roleId);
            return NotFound(new { message = "Role not found" });
        }

        return Ok(role);
    }

    [HttpGet("name/{roleName}")]
    public async Task<IActionResult> GetRoleByName([FromRoute] string roleName)
    {
        if (string.IsNullOrEmpty(roleName))
        {
            _logger.LogError("Role name cannot be null or empty");
            return BadRequest(new { message = "Role name cannot be null or empty" });
        }

        var role = await _roleService.GetRoleByNameAsync(roleName);
        if (role == null)
        {
            _logger.LogError("Role not found: {RoleName}", roleName);
            return NotFound(new { message = "Role not found" });
        }

        return Ok(role);
    }

    [HttpPut("{roleId}")]
    public async Task<IActionResult> UpdateRole([FromRoute] int roleId, [FromBody] Role role)
    {
        if (!ModelState.IsValid)
        {
            _logger.LogError("Invalid role data");
            return BadRequest(ModelState);
        }

        if (roleId <= 0)
        {
            _logger.LogError("Invalid role ID: {RoleId}", roleId);
            return BadRequest(new { message = "Invalid role ID" });
        }
        
        if (role.Id != roleId)
        {
            _logger.LogError("Role ID in route does not match body: Route={RoleId}, Body={BodyId}", roleId, role.Id);
            return BadRequest(new { message = "Role ID mismatch." });
        }


        bool updated = await _roleService.UpdateRoleAsync(role);
        if (!updated)
        {
            _logger.LogError("Failed to update role: {RoleName}", role.Name);
            return BadRequest(new { message = "Failed to update role, role name is not unique" });
        }

        _logger.LogInformation("Role updated successfully: {RoleName}", role.Name);
        return Ok(new { message = "Role updated successfully" });
    }

    [HttpDelete("{roleId}")]
    public async Task<IActionResult> DeleteRole([FromRoute] int roleId)
    {
        if (roleId <= 0)
        {
            _logger.LogError("Invalid role ID: {RoleId}", roleId);
            return BadRequest(new { message = "Invalid role ID" });
        }

        bool deleted = await _roleService.DeleteRoleAsync(roleId);
        if (deleted)
        {
            _logger.LogInformation("Role deleted successfully: {RoleId}", roleId);
            return Ok(new { message = "Role deleted successfully" });
        }

            _logger.LogError("Failed to delete role: {RoleId}", roleId);
            return BadRequest(new { message = "Failed to delete role, role is associated with users" });
        
    }
}

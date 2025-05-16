using Microsoft.AspNetCore.Mvc;
using BusinessLogic.Services;
using DataAccess.Models;
using EventBooking.RequestModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.Extensions.Logging;

namespace EventBooking.Controllers
{
    [Authorize(Roles = "1")]
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
            try
            {
                if (!ModelState.IsValid)
                {
                    _logger.LogError("Invalid role data");
                    return BadRequest(new { message = "Invalid role data", errors = ModelState });
                }

                if (string.IsNullOrWhiteSpace(role.Name))
                {
                    _logger.LogError("Role name is required");
                    return BadRequest(new { message = "Role name is required" });
                }

                _logger.LogInformation("Creating new role: {RoleName}", role.Name);
                int newRoleId = await _roleService.CreateRoleAsync(role);

                if (newRoleId > 0)
                {
                    _logger.LogInformation("Role created successfully: {RoleName}", role.Name);
                    return Ok(new
                    {
                        message = "Role created successfully",
                        roleId = newRoleId,
                        role = new { Id = newRoleId, Name = role.Name }
                    });
                }

                _logger.LogError("Failed to create role: {RoleName}", role.Name);
                return BadRequest(new { message = "Failed to create role, role name is not unique" });
            }
            catch (ArgumentException ex)
            {
                _logger.LogError(ex, "Error creating role: {RoleName}", role.Name);
                return BadRequest(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating role: {RoleName}", role.Name);
                return StatusCode(500, new { message = "Internal server error" });
            }
        }

        [HttpGet]
        public async Task<IActionResult> GetAllRoles()
        {
            try
            {
                _logger.LogInformation("Retrieving all roles");
                var roles = await _roleService.GetAllRolesAsync();

                if (roles.Count == 0)
                {
                    _logger.LogWarning("No roles found");
                    return NotFound(new { message = "No roles found" });
                }

                return Ok(new { message = "Roles retrieved successfully", roles });
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
            try
            {
                var role = await _roleService.GetRoleByIdAsync(roleId);
                return Ok(new { message = "Role retrieved successfully", role });
            }
            catch (KeyNotFoundException ex)
            {
                _logger.LogError(ex, "Error retrieving role by ID: {RoleId}", roleId);
                return NotFound(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving role by ID: {RoleId}", roleId);
                return StatusCode(500, new { message = "Internal server error" });
            }
        }

        [HttpGet("name/{roleName}")]
        public async Task<IActionResult> GetRoleByName([FromRoute] string roleName)
        {
            try
            {
                _logger.LogInformation("Retrieving role by name: {RoleName}", roleName);
                var role = await _roleService.GetRoleByNameAsync(roleName);
                return Ok(new { message = "Role retrieved successfully", role });
            }
            catch (KeyNotFoundException ex)
            {
                _logger.LogError(ex, "Error retrieving role by name: {RoleName}", roleName);
                return NotFound(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving role by name: {RoleName}", roleName);
                return StatusCode(500, new { message = "Internal server error" });
            }
        }

        [HttpPut("{roleId}")]
        public async Task<IActionResult> UpdateRole([FromRoute] int roleId, [FromBody] Role role)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    _logger.LogError("Invalid role data");
                    return BadRequest(new { message = "Invalid role data", errors = ModelState });
                }

                if (role.Id != roleId)
                {
                    _logger.LogError("Role ID in route does not match body: Route={RoleId}, Body={BodyId}", roleId, role.Id);
                    return BadRequest(new { message = "Role ID mismatch." });
                }

                bool updated = await _roleService.UpdateRoleAsync(role);
                if (updated)
                {
                    _logger.LogInformation("Role updated successfully: {RoleName}", role.Name);
                    return Ok(new { message = "Role updated successfully" });
                }

                _logger.LogError("Failed to update role: {RoleName}", role.Name);
                return BadRequest(new { message = "Failed to update role, role name is not unique" });
            }
            catch (ArgumentException ex)
            {
                _logger.LogError(ex, "Error updating role: {RoleName}", role.Name);
                return BadRequest(new { message = ex.Message });
            }
            catch (InvalidOperationException ex)
            {
                _logger.LogError(ex, "Error updating role: {RoleName}", role.Name);
                return BadRequest(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating role: {RoleName}", role.Name);
                return StatusCode(500, new { message = "Internal server error" });
            }
        }

        [HttpDelete("{roleId}")]
        public async Task<IActionResult> DeleteRole([FromRoute] int roleId)
        {
            try
            {
                _logger.LogInformation("Deleting role by ID: {RoleId}", roleId);
                bool deleted = await _roleService.DeleteRoleAsync(roleId);

                if (deleted)
                {
                    _logger.LogInformation("Role deleted successfully: {RoleId}", roleId);
                    return Ok(new { message = "Role deleted successfully" });
                }

                _logger.LogError("Failed to delete role: {RoleId}", roleId);
                return BadRequest(new { message = "Failed to delete role, role is associated with users" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting role: {RoleId}", roleId);
                return StatusCode(500, new { message = "Internal server error" });
            }
        }
    }
}
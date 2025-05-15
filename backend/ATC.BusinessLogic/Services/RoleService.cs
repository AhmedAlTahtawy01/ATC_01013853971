using ATC.DataAccess.Models;
using ATC.DataAccess.Repositories;
using Microsoft.Extensions.Logging;

namespace ATC.BusinessLogic.Services
{
    public class RoleService
    {
        private readonly RoleRepo _roleRepo;
        private readonly ILogger<RoleService> _logger;

        public RoleService(RoleRepo roleRepo, ILogger<RoleService> logger)
        {
            _roleRepo = roleRepo;
            _logger = logger;
        }

        // Validate role before creating or updating (Helper method)
        private void ValidateRole(Role role)
        {
            if (role == null)
            {
                _logger.LogError("Role cannot be null");
                throw new ArgumentNullException(nameof(role));
            }

            if (string.IsNullOrEmpty(role.Name))
            {
                _logger.LogError("Role name cannot be null or empty");
                throw new ArgumentException("Role name cannot be null or empty");
            }
        }

        // Create a new role
        public async Task<bool> CreateRoleAsync(Role role)
        {
            ValidateRole(role);

            try
            {
                _logger.LogInformation("Creating role: {RoleName}", role.Name);

                bool isUnique = await _roleRepo.IsRoleNameUniqueAsync(role.Name);
                if (!isUnique)
                {
                    _logger.LogWarning("Role name is not unique: {RoleName}", role.Name);
                    return false;
                }

                return await _roleRepo.CreateRoleAsync(role);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating role: {RoleName}", role.Name);
                throw;
            }
        }

        // Get all roles
        public async Task<List<Role>> GetAllRolesAsync()
        {
            try
            {
                _logger.LogInformation("Retrieving all roles");
                return await _roleRepo.GetAllRolesAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while retrieving all roles");
                throw;
            }
        }

        // Get role by ID
        public async Task<Role?> GetRoleByIdAsync(int roleId)
        {
            if (roleId <= 0)
            {
                _logger.LogError("Invalid role ID: {RoleId}", roleId);
                throw new ArgumentException("Role ID must be greater than 0");
            }

            try
            {
                _logger.LogInformation("Retrieving role by ID: {RoleId}", roleId);
                return await _roleRepo.GetRoleByIdAsync(roleId);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while retrieving role by ID: {RoleId}", roleId);
                throw;
            }
        }
        
        // Get role by Name
        public async Task<Role?> GetRoleByNameAsync(string roleName)
        {
            try
            {
                _logger.LogInformation("Retrieving role by Name: {RoleName}", roleName);
                return await _roleRepo.GetRoleByNameAsync(roleName);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while retrieving role by Name: {RoleName}", roleName);
                throw;
            }
        }

        // Update a role
        public async Task<bool> UpdateRoleAsync(Role role)
        {
            // Validate role ID before updating
            if (role.Id <= 0)
            {
                _logger.LogError("Invalid role ID: {RoleId}", role.Id);
                throw new ArgumentException("Role ID must be greater than 0");
            }

            // Validate role before updating
            ValidateRole(role);

            try
            {

                bool isUnique = await _roleRepo.IsRoleNameUniqueAsync(role.Name);
                if (!isUnique)
                {
                    _logger.LogWarning("Role name is not unique: {RoleName}", role.Name);
                    return false;
                }

                var existingRole = await _roleRepo.GetRoleByIdAsync(role.Id);
                if (existingRole == null)
                {
                    _logger.LogError("Role not found: {RoleId}", role.Id);
                    return false;
                }

                _logger.LogInformation("Updating role: {RoleName}", role.Name);
                return await _roleRepo.UpdateRoleAsync(role);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while updating role: {RoleName}", role.Name);
                throw;
            }
        }

        // Delete a role
        public async Task<bool> DeleteRoleAsync(int roleId)
        {
            try
            {
                _logger.LogInformation("Deleting role by ID: {RoleId}", roleId);
                return await _roleRepo.DeleteRoleAsync(roleId);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while deleting role: {RoleId}", roleId);
                throw;
            }
        }
    }
}
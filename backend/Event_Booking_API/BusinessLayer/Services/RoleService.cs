using DataAccess.Models;
using DataAccess.Repositories;
using Microsoft.Extensions.Logging;

namespace BusinessLogic.Services
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

            if (role.Name.Length > 20)
            {
                _logger.LogError("Role name cannot be longer than 20 characters");
                throw new ArgumentException("Role name cannot be longer than 20 characters");
            }
        }

        // Create a new role
        public async Task<int> CreateRoleAsync(Role role)
        {
            ValidateRole(role);

            try
            {
                _logger.LogInformation("Creating role: {RoleName}", role.Name);
                int newRoleId = await _roleRepo.CreateRoleAsync(role);

                if (newRoleId > 0)
                {
                    _logger.LogInformation("Role created successfully: {RoleName}", role.Name);
                    return newRoleId;
                }

                _logger.LogWarning("Failed to create role: {RoleName}", role.Name);
                return 0;
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
                var roleData = await _roleRepo.GetRoleByIdAsync(roleId);

                if (roleData == null)
                {
                    _logger.LogError("Role not found with ID: {RoleId}", roleId);
                    throw new KeyNotFoundException("Role not found");
                }

                return roleData;
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
                var roleData = await _roleRepo.GetRoleByNameAsync(roleName);

                if (roleData == null)
                {
                    _logger.LogError("Role not found with Name: {RoleName}", roleName);
                    throw new KeyNotFoundException("Role not found");
                }

                return roleData;
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
                var existingRole = await _roleRepo.GetRoleByIdAsync(role.Id);
                if (existingRole == null)
                {
                    _logger.LogError("Role not found: {RoleId}", role.Id);
                    throw new InvalidOperationException("Role not found");
                }

                bool isUnique = await _roleRepo.IsRoleNameUniqueAsync(role.Name);
                if (!isUnique && existingRole.Name != role.Name)
                {
                    _logger.LogWarning("Role name is not unique: {RoleName}", role.Name);
                    throw new InvalidOperationException("Role name is not unique");
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
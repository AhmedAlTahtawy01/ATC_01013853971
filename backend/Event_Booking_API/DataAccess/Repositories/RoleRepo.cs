using DataAccess.DataConnection;
using DataAccess.Models;
using Microsoft.Extensions.Logging;
using MySql.Data.MySqlClient;
using System.Data.Common;

namespace DataAccess.Repositories
{
    public class RoleRepo
    {
        private readonly ConnectionSettings _db;
        private readonly ILogger<RoleRepo> _logger;

        public RoleRepo(ConnectionSettings db, ILogger<RoleRepo> logger)
        {
            _db = db;
            _logger = logger;
        }

        // Create a new role
        public async Task<int> CreateRoleAsync(Role role)
        {
            const string query = @"
                INSERT INTO Roles (Name) 
                VALUES (@Name);
                SELECT LAST_INSERT_ID();";

            var param = new MySqlParameter("@Name", role.Name);

            try
            {
                _logger.LogInformation("Creating role: {RoleName}", role.Name);

                var id = await _db.ExecuteQueryAsync(query, async cmd =>
                {
                    return await cmd.ExecuteScalarAsync();
                }, param);

                // If id is not null, convert it to an int and return it
                if (id != null)
                {
                    int newRoleId = Convert.ToInt32(id);
                    _logger.LogInformation("Role created successfully: {RoleName}", role.Name);
                    return newRoleId;
                }

                // Another way to do it
                // return id != null ? Convert.ToInt32(id) : 0;
                // I have chosen the first way so that I can log the warning and return 0

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
        // No need for pagination here as there are only a few roles
        public async Task<List<Role>> GetAllRolesAsync()
        {
            const string query = "SELECT * FROM Roles";

            try
            {
                _logger.LogInformation("Retrieving all roles");

                return await _db.ExecuteQueryAsync(query, async cmd =>
                {
                    var roles = new List<Role>();
                    using var reader = await cmd.ExecuteReaderAsync();

                    while (await reader.ReadAsync())
                    {
                        roles.Add(MapRole(reader));
                    }

                    _logger.LogInformation("Retrieved {Count} roles from the database.", roles.Count);
                    return roles;
                });

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
            const string query = "SELECT * FROM Roles WHERE Id = @Id";

            var param = new MySqlParameter("@Id", roleId);

            try
            {
                _logger.LogInformation("Retrieving role by ID: {RoleId}", roleId);

                return await _db.ExecuteQueryAsync(query, async cmd =>
                {
                    using var reader = await cmd.ExecuteReaderAsync();
                    if (await reader.ReadAsync())
                    {
                        _logger.LogInformation("Role found: {RoleId}", roleId);
                        return MapRole(reader);
                    }

                    _logger.LogWarning("No role found with ID: {RoleId}", roleId);
                    return null;
                }, param);
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
            const string query = "SELECT * FROM Roles WHERE Name = @Name";
            var param = new MySqlParameter("@Name", roleName);

            try
            {
                _logger.LogInformation("Retrieving role by Name: {RoleName}", roleName);

                return await _db.ExecuteQueryAsync(query, async cmd =>
                {
                    using var reader = await cmd.ExecuteReaderAsync();
                    if (await reader.ReadAsync())
                    {
                        _logger.LogInformation("Role found: {RoleName}", roleName);
                        return MapRole(reader);
                    }

                    _logger.LogWarning("No role found with Name: {RoleName}", roleName);
                    return null;
                }, param);
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
            const string query = @"
                UPDATE Roles
                SET Name = @Name
                WHERE Id = @Id";

            var parameters = new MySqlParameter[]
            {
                new MySqlParameter("@Name", role.Name),
                new MySqlParameter("@Id", role.Id)
            };

            try
            {
                _logger.LogInformation("Updating role: {RoleName}", role.Name);

                int rowsAffected = await _db.ExecuteQueryAsync(query, async cmd =>
                {
                    return await cmd.ExecuteNonQueryAsync();
                }, parameters);

                if (rowsAffected == 1)
                {
                    _logger.LogInformation("Role updated successfully: {RoleName}", role.Name);
                    return true;
                }

                if (rowsAffected > 1)
                {
                    _logger.LogError("Multiple roles updated with the same ID: {RoleId}", role.Id);
                    throw new InvalidOperationException("Multiple roles updated with the same ID.");
                }

                _logger.LogWarning("Failed to update role: {RoleName}", role.Name);
                return false;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating role: {RoleName}", role.Name);
                throw;
            }
        }

        // Delete a role
        public async Task<bool> DeleteRoleAsync(int roleId)
        {
            const string query = "DELETE FROM Roles WHERE Id = @Id";

            var param = new MySqlParameter("@Id", roleId);

            try
            {
                _logger.LogInformation("Deleting role by ID: {RoleId}", roleId);

                int rowsAffected = await _db.ExecuteQueryAsync(query, async cmd =>
                {
                    return await cmd.ExecuteNonQueryAsync();
                }, param);

                if (rowsAffected == 1)
                {
                    _logger.LogInformation("Role deleted successfully: {RoleId}", roleId);
                    return true;
                }

                if (rowsAffected > 1)
                {
                    _logger.LogError("Multiple roles deleted with the same ID: {RoleId}", roleId);
                    throw new InvalidOperationException("Multiple roles deleted with the same ID.");
                }

                _logger.LogWarning("No role deleted with ID: {RoleId}", roleId);
                return false;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while deleting role: {RoleId}", roleId);
                throw;
            }
        }

        // Check if role name is unique
        public async Task<bool> IsRoleNameUniqueAsync(string roleName)
        {
            const string query = "SELECT COUNT(*) FROM Roles WHERE Name = @Name";
            var param = new MySqlParameter("@Name", roleName);

            try
            {
                _logger.LogInformation("Checking if role name is unique: {RoleName}", roleName);

                var count = await _db.ExecuteQueryAsync(query, async cmd =>
                {
                    return await cmd.ExecuteScalarAsync();
                }, param);

                if (count == null)
                {
                    _logger.LogWarning("Unexpected null count when checking role name uniqueness: {RoleName}", roleName);
                    return false;
                }

                var roleCount = Convert.ToInt32(count);
                return roleCount == 0;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error checking if role name is unique: {RoleName}", roleName);
                throw;
            }
        }

        // Map role from database reader (Helper method)
        private Role MapRole(DbDataReader reader)
        {
            var idOrdinal = reader.GetOrdinal("Id");
            var nameOrdinal = reader.GetOrdinal("Name");

            return new Role
            {
                Id = reader.GetInt32(idOrdinal),
                Name = reader.GetString(nameOrdinal)
            };
        }
    }
}

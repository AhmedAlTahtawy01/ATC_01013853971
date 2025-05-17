using DataAccess.DataConnection;
using DataAccess.Models;
using Microsoft.Extensions.Logging;
using MySql.Data.MySqlClient;
using System.Data.Common;

namespace DataAccess.Repositories
{
    public class UserRepo
    {
        private readonly ConnectionSettings _db;
        private readonly ILogger<UserRepo> _logger;

        public UserRepo(ConnectionSettings connectionSettings, ILogger<UserRepo> logger)
        {
            _db = connectionSettings;
            _logger = logger;
        }

        // Create user (Register)
        public async Task<int> CreateUserAsync(User user)
        {
            const string query = @"
                INSERT INTO Users (Username, Name, Email, PasswordHash, RoleId)
                VALUES (@Username, @Name, @Email, @PasswordHash, @RoleId);
                SELECT LAST_INSERT_ID();";

            var parameters = new MySqlParameter[]
            {
                new MySqlParameter("@Username", user.Username),
                new MySqlParameter("@Name", user.Name),
                new MySqlParameter("@Email", user.Email),
                new MySqlParameter("@PasswordHash", user.PasswordHash),
                new MySqlParameter("@RoleId", user.RoleId)
            };

            try
            {
                _logger.LogInformation("Creating user with username: {Username}", user.Username);

                var id = await _db.ExecuteQueryAsync(query, async cmd =>
                {
                    return await cmd.ExecuteScalarAsync();
                }, parameters);

                if (id != null)
                {
                    int newUserId = Convert.ToInt32(id);
                    _logger.LogInformation("User created successfully: {Username}", user.Username);
                    return newUserId;
                }

                _logger.LogWarning("Failed to create user. No rows affected.");
                return 0;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while creating user: {Username}", user.Username);
                throw;
            }
        }

        // Get all users (Pagination)
        public async Task<List<User>> GetAllUsersAsync(int pageNumber = 1, int pageSize = 10)
        {
            const string query = @"
                SELECT * FROM Users
                ORDER BY Name
                LIMIT @PageSize OFFSET @Offset";

            var offset = (pageNumber - 1) * pageSize;
            var parameters = new MySqlParameter[]
            {
                new MySqlParameter("@PageSize", pageSize),
                new MySqlParameter("@Offset", offset)
            };

            try
            {
                _logger.LogInformation("Retrieving users: Page {PageNumber}, PageSize {PageSize}", pageNumber, pageSize);

                return await _db.ExecuteQueryAsync(query, async cmd =>
                {
                    var users = new List<User>();
                    using var reader = await cmd.ExecuteReaderAsync();

                    while (await reader.ReadAsync())
                    {
                        users.Add(MapUser(reader));
                    }

                    _logger.LogInformation("Retrieved {Count} users from the database.", users.Count);
                    return users;
                }, parameters);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while retrieving all users.");
                throw;
            }
        }

        // Count all users in the system
        public async Task<int> CountUsersAsync()
        {
            const string query = "SELECT COUNT(*) FROM Users";

            try
            {
                _logger.LogInformation("Counting all users in the system");


                var usersCount = await _db.ExecuteQueryAsync(query, async cmd =>
                {
                    return await cmd.ExecuteScalarAsync();
                });

                if (usersCount != null)
                {
                    _logger.LogInformation("Total users in the system: {Count}", usersCount);
                    return Convert.ToInt32(usersCount);
                }

                _logger.LogWarning("No users found in the system");
                return 0;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while counting all users in the system");
                throw;
            }
        }


        // Get user by column (Helper method)
        private async Task<User?> GetByColumnAsync(string column, string value)
        {
            // Whitelist of allowed column names
            var allowedColumns = new HashSet<string> { "Username", "Email" };

            if (!allowedColumns.Contains(column))
                throw new ArgumentException($"Invalid column name: {column}");

            // Safe string interpolation AFTER validation
            string query = $"SELECT * FROM Users WHERE {column} = @Value";
            var param = new MySqlParameter("@Value", value);

            try
            {
                _logger.LogInformation("Retrieving user by {Column}: {Value}", column, value);

                return await _db.ExecuteQueryAsync(query, async cmd =>
                {
                    using var reader = await cmd.ExecuteReaderAsync();
                    if (await reader.ReadAsync())
                    {
                        _logger.LogInformation("User found: {Column} = {Value}", column, value);
                        return MapUser(reader);
                    }

                    _logger.LogWarning("No user found with {Column}: {Value}", column, value);
                    return null;
                }, param);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while retrieving user by {Column}: {Value}", column, value);
                throw;
            }
        }

        // Get user by ID
        public async Task<User?> GetByIdAsync(int userId)
        {
            const string query = @"
                SELECT * FROM Users 
                WHERE Id = @UserId";

            var param = new MySqlParameter("@UserId", userId);

            try
            {
                _logger.LogInformation("Retrieving user by ID: {UserId}", userId);

                return await _db.ExecuteQueryAsync(query, async cmd =>
                {
                    using var reader = await cmd.ExecuteReaderAsync();
                    if (await reader.ReadAsync())
                    {
                        _logger.LogInformation("User found: {UserId}", userId);
                        return MapUser(reader);
                    }

                    _logger.LogWarning("No user found with ID: {UserId}", userId);
                    return null;
                }, param);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while retrieving user by ID: {UserId}", userId);
                throw;
            }
        }

        // Get user by username
        public async Task<User?> GetByUsernameAsync(string username)
        {
            return await GetByColumnAsync("Username", username);
        }

        // Get user by email
        public async Task<User?> GetByEmailAsync(string email)
        {
            return await GetByColumnAsync("Email", email);
        }

        // Update user (Update user name and password only)
        public async Task<bool> UpdateUserAsync(User user)
        {
            const string query = @"
                UPDATE Users
                SET Name = @Name, PasswordHash = @PasswordHash
                WHERE Id = @Id";

            var parameters = new MySqlParameter[]
            {
                new MySqlParameter("@Name", user.Name),
                new MySqlParameter("@PasswordHash", user.PasswordHash),
                new MySqlParameter("@Id", user.Id)
            };

            try
            {
                _logger.LogInformation("Updating user: {UserId}", user.Id);

                int rowsAffected = await _db.ExecuteQueryAsync(query, async cmd =>
                {
                    return await cmd.ExecuteNonQueryAsync();
                }, parameters);

                if (rowsAffected == 1)
                {
                    _logger.LogInformation("User with ID: {UserId} updated successfully", user.Id);
                    return true;
                }

                if (rowsAffected > 1)
                {
                    _logger.LogError("Multiple users updated with the same ID: {UserId}", user.Id);
                    throw new InvalidOperationException("Multiple users updated with the same ID.");
                }

                _logger.LogWarning("User update failed or no changes made for ID: {UserId}", user.Id);
                return false;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while updating user with ID: {UserId}", user.Id);
                throw;
            }
        }

        // Change user role (For admins only)
        public async Task<bool> ChangeUserRoleAsync(int userId, int newRoleId)
        {
            const string query = @"
                UPDATE Users
                SET RoleId = @NewRoleId
                WHERE Id = @UserId";

            var parameters = new MySqlParameter[]
            {
                new MySqlParameter("@NewRoleId", newRoleId),
                new MySqlParameter("@UserId", userId)
            };

            try
            {
                _logger.LogInformation("Changing user role for user ID: {UserId} to role ID: {NewRoleId}", userId, newRoleId);

                int rowsAffected = await _db.ExecuteQueryAsync(query, async cmd =>
                {
                    return await cmd.ExecuteNonQueryAsync();
                }, parameters);

                if (rowsAffected == 1)
                {
                    _logger.LogInformation("User role changed successfully for user ID: {UserId}", userId);
                    return true;
                }

                if (rowsAffected > 1)
                {
                    _logger.LogError("Multiple users updated with the same ID: {UserId}", userId);
                    throw new InvalidOperationException("Multiple users updated with the same ID.");
                }

                _logger.LogWarning("User role change failed or no changes made for user ID {UserId}", userId);
                return false;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while changing user role: {UserId}", userId);
                throw;
            }
        }

        // Delete user
        public async Task<bool> DeleteUserAsync(int userId)
        {
            const string query = "DELETE FROM Users WHERE Id = @UserId";
            var param = new MySqlParameter("@UserId", userId);

            try
            {
                _logger.LogInformation("Deleting user by ID: {UserId}", userId);

                int rowsAffected = await _db.ExecuteQueryAsync(query, async cmd =>
                {
                    return await cmd.ExecuteNonQueryAsync();
                }, param);

                if (rowsAffected == 1)
                {
                    _logger.LogInformation("User deleted successfully: {UserId}", userId);
                    return true;
                }

                if (rowsAffected > 1)
                {
                    _logger.LogError("Multiple users deleted with the same ID: {UserId}", userId);
                    throw new InvalidOperationException("Multiple users deleted with the same ID.");
                }

                _logger.LogWarning("No user deleted with ID: {UserId}", userId);
                return false;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while deleting user: {UserId}", userId);
                throw;
            }
        }

        // Validate user credentials
        public async Task<User?> ValidateCredentialsAsync(string username, string passwordHash)
        {
            try
            {
                _logger.LogInformation("Validating credentials for user: {Username}", username);

                var user = await GetByUsernameAsync(username);
                if (user == null)
                {
                    _logger.LogWarning("User not found: {Username}", username);
                    return null;
                }

                if (user.PasswordHash == null)
                {
                    _logger.LogWarning("User {Username} has no password hash set.", username);
                    return null;
                }

                if (user.PasswordHash == passwordHash)
                {
                    _logger.LogInformation("Credentials validated successfully for user: {Username}", username);
                    return user;
                }

                _logger.LogWarning("Invalid password for user: {Username}", username);
                return null;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while validating credentials for user: {Username}", username);
                throw;
            }
        }

        // Map user from database reader (Helper method)
        private User MapUser(DbDataReader reader)
        {
            var idOrdinal = reader.GetOrdinal("Id");
            var usernameOrdinal = reader.GetOrdinal("Username");
            var nameOrdinal = reader.GetOrdinal("Name");
            var emailOrdinal = reader.GetOrdinal("Email");
            var passwordHashOrdinal = reader.GetOrdinal("PasswordHash");
            var roleIdOrdinal = reader.GetOrdinal("RoleId");

            return new User
            {
                Id = reader.GetInt32(idOrdinal),
                Username = reader.GetString(usernameOrdinal),
                Name = reader.GetString(nameOrdinal),
                Email = reader.GetString(emailOrdinal),
                PasswordHash = reader.GetString(passwordHashOrdinal),
                RoleId = reader.GetInt32(roleIdOrdinal)
            };
        }
    }
}
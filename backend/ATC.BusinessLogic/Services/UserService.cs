using ATC.DataAccess.Models;
using ATC.DataAccess.Repositories;
using Microsoft.Extensions.Logging;
using BCrypt.Net;

namespace ATC.BusinessLogic.Services
{
    public class UserService
    {
        private readonly UserRepo _userRepo;
        private readonly ILogger<UserService> _logger;
        
        public UserService(UserRepo userRepo, ILogger<UserService> logger)
        {
            _userRepo = userRepo;
            _logger = logger;
        }
        
        // Validate user before creating or updating (Helper method)
        private void ValidateUser(User user)
        {
            if (user == null)
            {
                _logger.LogError("User cannot be null");
                throw new ArgumentNullException(nameof(user));
            }

            if (string.IsNullOrEmpty(user.Username))
            {
                _logger.LogError("Username cannot be null or empty");
                throw new ArgumentException("Username cannot be null or empty");
            }

            if (string.IsNullOrEmpty(user.Email))
            {
                _logger.LogError("Email cannot be null or empty");
                throw new ArgumentException("Email cannot be null or empty");
            }
            
            if (string.IsNullOrEmpty(user.PasswordHash))
            {
                _logger.LogError("Password cannot be null or empty");
                throw new ArgumentException("Password cannot be null or empty");
            }
        }
        
        // Hash the password
        private string HashPassword(string plainPassword) =>
         BCrypt.Net.BCrypt.HashPassword(plainPassword);

        // Verify the password
        private bool VerifyPassword(string hashedPassword, string plainPassword) =>
         BCrypt.Net.BCrypt.Verify(plainPassword, hashedPassword);

        // Register a new user
        public async Task<bool> CreateUserAsync(User user)
        {
            ValidateUser(user);

            try
            {
                _logger.LogInformation("Starting user registration for: {Username}", user.Username);

                // Check if username or email already exists
                var existingByUsername = await _userRepo.GetByUsernameAsync(user.Username);
                var existingByEmail = await _userRepo.GetByEmailAsync(user.Email);

                if (existingByUsername != null)
                {
                    _logger.LogWarning("Username already exists: {Username}", user.Username);
                    return false;
                }
                
                if (existingByEmail != null)
                {
                    _logger.LogWarning("Email already exists: {Email}", user.Email);
                    return false;
                }
                
                // Hash the password
                user.PasswordHash = HashPassword(user.PasswordHash);

                if (user.RoleId == 0)
                {
                    user.RoleId = 2; // Default role ID (User)
                }

                // Save the user
                bool created = await _userRepo.CreateUserAsync(user);

                if (created)
                {
                    _logger.LogInformation("User registered successfully: {Username}", user.Username);
                    return true;
                }
                
                _logger.LogWarning("Failed to register user: {Username}", user.Username);
                return false;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error during user registration for: {Username}", user.Username);
                throw;
            }
        }

        // Login (validate credentials)
        public async Task<User?> LoginAsync(string username, string password)
        {
            if (string.IsNullOrEmpty(username) || string.IsNullOrEmpty(password))
            {
                _logger.LogError("Username and password are required");
                throw new ArgumentException("Username and password are required");
            }

            var user = await _userRepo.GetByUsernameAsync(username);
            if (user == null)
            {
                _logger.LogWarning("User not found: {Username}", username);
                return null;
            }

            try
            {
                if (user.PasswordHash == null || !BCrypt.Net.BCrypt.Verify(password, user.PasswordHash))
                {
                    _logger.LogWarning("Invalid password for user: {Username}", username);
                    return null;
                }

                return user;
            }
            catch (SaltParseException ex)
            {
                _logger.LogError(ex, "Corrupted password hash for user: {Username}", username);
                return null;
            }
        }

        // Get all users with pagination
        public async Task<List<User>> GetAllUsersAsync(int pageNumber = 1, int pageSize = 10)
        {
            if (pageNumber <= 0 || pageSize <= 0)
            {
                _logger.LogError("Invalid page number or page size");
                throw new ArgumentException("Page number and page size must be greater than 0");
            }

            try
            {
                _logger.LogInformation("Retrieving all users with pagination");
                return await _userRepo.GetAllUsersAsync(pageNumber, pageSize);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while retrieving all users with pagination");
                throw;
            }
        }
        
        // Get user by ID
        public async Task<User?> GetByIdAsync(int userId)
        {
            if (userId <= 0)
            {
                _logger.LogError("Invalid user ID: {UserId}", userId);
                throw new ArgumentException("User ID must be greater than 0");
            }

            try
            {
                _logger.LogInformation("Retrieving user by ID: {UserId}", userId);
                return await _userRepo.GetByIdAsync(userId);
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
            if (string.IsNullOrEmpty(username))
            {
                _logger.LogError("Username is required");
                throw new ArgumentException("Username is required");
            }

            try
            {
                _logger.LogInformation("Retrieving user by username: {Username}", username);
                return await _userRepo.GetByUsernameAsync(username);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while retrieving user by username: {Username}", username);
                throw;
            }
        }

        // Get user by email
        public async Task<User?> GetByEmailAsync(string email)
        {
            if (string.IsNullOrEmpty(email))
            {
                _logger.LogError("Email is required");
                throw new ArgumentException("Email is required");
            }

            try
            {
                _logger.LogInformation("Retrieving user by email: {Email}", email);
                return await _userRepo.GetByEmailAsync(email);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while retrieving user by email: {Email}", email);
                throw;
            }
        }
        
        // Update user
        public async Task<bool> UpdateUserAsync(User user)
        {
            if (user.Id <= 0)
            {
                _logger.LogError("Invalid user ID: {UserId}", user.Id);
                throw new ArgumentException("User ID must be greater than 0");
            }

            ValidateUser(user);

            var existingUser = await _userRepo.GetByIdAsync(user.Id);
            if (existingUser == null)
            {
                _logger.LogError("User not found: {UserId}", user.Id);
                throw new InvalidOperationException("User not found");
            }

            // Hash password if it's being updated
            if (!string.IsNullOrEmpty(user.PasswordHash) && HashPassword(user.PasswordHash) != existingUser.PasswordHash)
            {
                user.PasswordHash = HashPassword(user.PasswordHash);
            }

            try
            {
                _logger.LogInformation("Updating user: {UserId}", user.Id);
                return await _userRepo.UpdateUserAsync(user);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while updating user: {UserId}", user.Id);
                throw;
            }
        }

        // Change user role (For admins only)
        public async Task<bool> ChangeUserRoleAsync(int userId, int newRoleId)
        {
            if (userId <= 0 || newRoleId <= 0)
            {
                _logger.LogError("Invalid user ID or new role ID");
                throw new ArgumentException("User ID and new role ID must be greater than 0");
            }

            try
            {
                _logger.LogInformation("Changing user role for: {UserId} to {NewRoleId}", userId, newRoleId);
                return await _userRepo.ChangeUserRoleAsync(userId, newRoleId);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while changing user role: {UserId} to {NewRoleId}", userId, newRoleId);
                throw;
            }
        }
        
        // Delete user
        public async Task<bool> DeleteUserAsync(int userId)
        {
            if (userId <= 0)
            {
                _logger.LogError("Invalid user ID: {UserId}", userId);
                throw new ArgumentException("User ID must be greater than 0");
            }

            try
            {
                _logger.LogInformation("Deleting user by ID: {UserId}", userId);
                return await _userRepo.DeleteUserAsync(userId);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while deleting user: {UserId}", userId);
                throw;
            }
        }
    }
}

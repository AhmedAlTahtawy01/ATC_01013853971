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
        
        private string HashPassword(string plainPassword) =>
         BCrypt.Net.BCrypt.HashPassword(plainPassword);

        private bool VerifyPassword(string hashedPassword, string plainPassword) =>
         BCrypt.Net.BCrypt.Verify(plainPassword, hashedPassword);

        // Register a new user
        public async Task<bool> CreateUserAsync(User user)
        {
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

            return await _userRepo.GetAllUsersAsync(pageNumber, pageSize);
        }
        
        // Get user by ID
        public async Task<User?> GetByIdAsync(int userId)
        {
            if (userId <= 0)
            {
                _logger.LogError("Invalid user ID: {UserId}", userId);
                throw new ArgumentException("User ID must be greater than 0");
            }

            return await _userRepo.GetByIdAsync(userId);
        }

        // Get user by username
        public async Task<User?> GetByUsernameAsync(string username)
        {
            if (string.IsNullOrEmpty(username))
            {
                _logger.LogError("Username is required");
                throw new ArgumentException("Username is required");
            }

            return await _userRepo.GetByUsernameAsync(username);
        }
        
        // Get user by email
        public async Task<User?> GetByEmailAsync(string email)
        {
            if (string.IsNullOrEmpty(email))
            {
                _logger.LogError("Email is required");
                throw new ArgumentException("Email is required");
            }

            return await _userRepo.GetByEmailAsync(email);
        }
        
        // Update user
        public async Task<bool> UpdateUserAsync(User user)
        {
            if (user == null)
            {
                _logger.LogError("User cannot be null");
                throw new ArgumentNullException(nameof(user));
            }

            if (user.Id <= 0)
            {
                _logger.LogError("Invalid user ID: {UserId}", user.Id);
                throw new ArgumentException("User ID must be greater than 0");
            }

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

            return await _userRepo.UpdateUserAsync(user);
        }

        // Change user role (For admins only)
        public async Task<bool> ChangeUserRoleAsync(int userId, int newRoleId)
        {
            if (userId <= 0 || newRoleId <= 0)
            {
                _logger.LogError("Invalid user ID or new role ID");
                throw new ArgumentException("User ID and new role ID must be greater than 0");
            }

            return await _userRepo.ChangeUserRoleAsync(userId, newRoleId);
        }
        
        // Delete user
        public async Task<bool> DeleteUserAsync(int userId)
        {
            if (userId <= 0)
            {
                _logger.LogError("Invalid user ID: {UserId}", userId);
                throw new ArgumentException("User ID must be greater than 0");
            }

            return await _userRepo.DeleteUserAsync(userId);
        }
    }
}

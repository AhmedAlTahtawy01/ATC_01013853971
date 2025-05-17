using DataAccess.DataConnection;
using DataAccess.Models;
using Microsoft.Extensions.Logging;
using MySql.Data.MySqlClient;
using System.Data.Common;

namespace DataAccess.Repositories
{
    public class TagRepo
    {
        private readonly ConnectionSettings _db;
        private readonly ILogger<TagRepo> _logger;

        public TagRepo(ConnectionSettings db, ILogger<TagRepo> logger)
        {
            _db = db;
            _logger = logger;
        }

        // Create tag
        public async Task<int> CreateTagAsync(Tag tag)
        {
            const string query = @"
                INSERT INTO Tags (Name)
                VALUES (@Name);
                SELECT LAST_INSERT_ID();";

            var param = new MySqlParameter("@Name", tag.Name);

            try
            {
                _logger.LogInformation("Creating tag: {TagName}", tag.Name);

                var id = await _db.ExecuteQueryAsync(query, async cmd =>
                {
                    return await cmd.ExecuteScalarAsync();
                }, param);

                if (id != null)
                {
                    _logger.LogInformation("Tag created successfully: {TagId}", id);
                    return Convert.ToInt32(id);
                }

                _logger.LogWarning("Failed to create tag. No rows affected.");
                return 0;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while creating tag: {TagName}", tag.Name);
                throw;
            }
        }

        // Get all tags (Pagination)
        public async Task<List<Tag>> GetAllTagsAsync(int pageNumber = 1, int pageSize = 10)
        {
            const string query = @"
                SELECT * FROM Tags
                ORDER BY Name ASC
                LIMIT @PageSize OFFSET @Offset";

            var parameters = new MySqlParameter[]
            {
                new MySqlParameter("@PageSize", pageSize),
                new MySqlParameter("@Offset", (pageNumber - 1) * pageSize)
            };

            try
            {
                _logger.LogInformation("Retrieving all tags (page {PageNumber}, size {PageSize})", pageNumber, pageSize);

                return await _db.ExecuteQueryAsync(query, async cmd =>
                {
                    var reader = await cmd.ExecuteReaderAsync();
                    var tagsList = await MapTags(reader);

                    _logger.LogInformation("Retrieved {Count} tags from the database.", tagsList.Count);
                    return tagsList;
                }, parameters);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while retrieving all tags");
                throw;
            }
        }

        // Get tag by ID
        public async Task<Tag?> GetTagByIdAsync(int tagId)
        {
            const string query = @"
                SELECT * FROM Tags 
                WHERE Id = @TagId";

            var param = new MySqlParameter("@TagId", tagId);

            try
            {
                _logger.LogInformation("Retrieving tag by ID: {TagId}", tagId);

                return await _db.ExecuteQueryAsync(query, async cmd =>
                {
                    using var reader = await cmd.ExecuteReaderAsync();
                    if (await reader.ReadAsync())
                    {
                        _logger.LogInformation("Tag found: {TagId}", tagId);
                        return MapTag(reader);
                    }

                    _logger.LogWarning("No tag found with ID: {TagId}", tagId);
                    return null;
                }, param);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while retrieving tag by ID: {TagId}", tagId);
                throw;
            }
        }

        // Get tag by name
        public async Task<Tag?> GetTagByNameAsync(string name)
        {
            const string query = "SELECT * FROM Tags WHERE Name = @Name";
            var param = new MySqlParameter("@Name", name);

            try
            {
                _logger.LogInformation("Retrieving tag by name: {Name}", name);

                return await _db.ExecuteQueryAsync(query, async cmd =>
                {
                    using var reader = await cmd.ExecuteReaderAsync();
                    if (await reader.ReadAsync())
                    {
                        _logger.LogInformation("Tag found: {Name}", name);
                        return MapTag(reader);
                    }

                    _logger.LogWarning("No tag found with name: {Name}", name);
                    return null;
                }, param);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while retrieving tag by name: {Name}", name);
                throw;
            }
        }

        // Update tag
        public async Task<bool> UpdateTagAsync(Tag tag)
        {
            const string query = @"
                UPDATE Tags 
                SET Name = @Name 
                WHERE Id = @TagId";

            var parameters = new MySqlParameter[]
            {
                new MySqlParameter("@Name", tag.Name),
                new MySqlParameter("@TagId", tag.Id)
            };

            try
            {
                _logger.LogInformation("Updating tag: {TagId}", tag.Id);

                int rowsAffected = await _db.ExecuteQueryAsync(query, async cmd =>
                {
                    return await cmd.ExecuteNonQueryAsync();
                }, parameters);

                if (rowsAffected == 1)
                {
                    _logger.LogInformation("Tag updated successfully: {TagId}", tag.Id);
                    return true;
                }

                if (rowsAffected > 1)
                {
                    _logger.LogError("Multiple tags updated with the same ID: {TagId}", tag.Id);
                    throw new InvalidOperationException("Multiple tags updated with the same ID.");
                }

                _logger.LogWarning("Tag update failed or no changes made for ID: {TagId}", tag.Id);
                return false;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while updating tag: {TagId}", tag.Id);
                throw;
            }
        }

        // Delete tag
        public async Task<bool> DeleteTagAsync(int tagId)
        {
            const string query = "DELETE FROM Tags WHERE Id = @TagId";
            var param = new MySqlParameter("@TagId", tagId);

            try
            {
                _logger.LogInformation("Deleting tag: {TagId}", tagId);

                int rowsAffected = await _db.ExecuteQueryAsync(query, async cmd =>
                {
                    return await cmd.ExecuteNonQueryAsync();
                }, param);

                if (rowsAffected == 1)
                {
                    _logger.LogInformation("Tag deleted successfully: {TagId}", tagId);
                    return true;
                }

                if (rowsAffected > 1)
                {
                    _logger.LogError("Multiple tags deleted with the same ID: {TagId}", tagId);
                    throw new InvalidOperationException("Multiple tags deleted with the same ID.");
                }

                _logger.LogWarning("No tag deleted with ID: {TagId}", tagId);
                return false;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while deleting tag: {TagId}", tagId);
                throw;
            }
        }

        // Map tag from database reader (Helper method)
        private Tag MapTag(DbDataReader reader)
        {
            int idOrdinal = reader.GetOrdinal("Id");
            int nameOrdinal = reader.GetOrdinal("Name");

            return new Tag
            {
                Id = reader.GetInt32(idOrdinal),
                Name = reader.GetString(nameOrdinal)
            };
        }

        // Map tags from database reader (Helper method)
        private async Task<List<Tag>> MapTags(DbDataReader reader)
        {
            var tags = new List<Tag>();
            while (await reader.ReadAsync())
            {
                tags.Add(MapTag(reader));
            }
            return tags;
        }
    }
}
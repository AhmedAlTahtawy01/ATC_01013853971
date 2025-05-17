using DataAccess.DataConnection;
using DataAccess.Models;
using Microsoft.Extensions.Logging;
using MySql.Data.MySqlClient;
using System.Data.Common;

namespace DataAccess.Repositories
{
    public class EventTagRepo
    {
        private readonly ConnectionSettings _db;
        private readonly ILogger<EventTagRepo> _logger;

        public EventTagRepo(ConnectionSettings db, ILogger<EventTagRepo> logger)
        {
            _db = db;
            _logger = logger;
        }

        // Create event tag
        public async Task<bool> CreateEventTagAsync(EventTag eventTag)
        {
            const string query = @"
                INSERT INTO EventTags (EventId, TagId)
                VALUES (@EventId, @TagId)";

            var parameters = new MySqlParameter[]
            {
                new MySqlParameter("@EventId", eventTag.EventId),
                new MySqlParameter("@TagId", eventTag.TagId)
            };

            try
            {
                _logger.LogInformation("Creating event tag: EventId={EventId}, TagId={TagId}", eventTag.EventId, eventTag.TagId);

                int rowsAffected = await _db.ExecuteQueryAsync(query, async cmd =>
                {
                    return await cmd.ExecuteNonQueryAsync();
                }, parameters);

                if (rowsAffected == 1)
                {
                    _logger.LogInformation("Event tag created successfully: EventId={EventId}, TagId={TagId}", eventTag.EventId, eventTag.TagId);
                    return true;
                }

                _logger.LogWarning("Failed to create event tag: EventId={EventId}, TagId={TagId}", eventTag.EventId, eventTag.TagId);
                return false;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while creating event tag: EventId={EventId}, TagId={TagId}", eventTag.EventId, eventTag.TagId);
                throw;
            }
        }

        // Get all event tags
        public async Task<List<EventTag>> GetAllEventTagsAsync()
        {
            const string query = "SELECT * FROM EventTags";

            try
            {
                _logger.LogInformation("Retrieving all event tags");

                return await _db.ExecuteQueryAsync(query, async cmd =>
                {
                    var reader = await cmd.ExecuteReaderAsync();
                    var eventTagsList = await MapEventTags(reader);

                    _logger.LogInformation("Retrieved {Count} event tags from the database.", eventTagsList.Count);
                    return eventTagsList;
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while retrieving all event tags");
                throw;
            }
        }

        // Get event tags by event ID
        public async Task<List<EventTag>> GetEventTagsByEventIdAsync(int eventId)
        {
            const string query = "SELECT * FROM EventTags WHERE EventId = @EventId";
            var param = new MySqlParameter("@EventId", eventId);

            try
            {
                _logger.LogInformation("Retrieving event tags by EventId: {EventId}", eventId);

                return await _db.ExecuteQueryAsync(query, async cmd =>
                {
                    var reader = await cmd.ExecuteReaderAsync();
                    var eventTagsList = await MapEventTags(reader);

                    _logger.LogInformation("Retrieved {Count} event tags for EventId: {EventId}", eventTagsList.Count, eventId);
                    return eventTagsList;
                }, param);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while retrieving event tags by EventId: {EventId}", eventId);
                throw;
            }
        }

        // Get event tags by tag ID
        public async Task<List<EventTag>> GetEventTagsByTagIdAsync(int tagId)
        {
            const string query = "SELECT * FROM EventTags WHERE TagId = @TagId";
            var param = new MySqlParameter("@TagId", tagId);

            try
            {
                _logger.LogInformation("Retrieving event tags by TagId: {TagId}", tagId);

                return await _db.ExecuteQueryAsync(query, async cmd =>
                {
                    var reader = await cmd.ExecuteReaderAsync();
                    var eventTagsList = await MapEventTags(reader);

                    _logger.LogInformation("Retrieved {Count} event tags for TagId: {TagId}", eventTagsList.Count, tagId);
                    return eventTagsList;
                }, param);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while retrieving event tags by TagId: {TagId}", tagId);
                throw;
            }
        }

        // Check if event tag exists
        public async Task<bool> CheckEventTagExistsAsync(int eventId, int tagId)
        {
            const string query = @"
                SELECT COUNT(*) FROM EventTags 
                WHERE EventId = @EventId AND TagId = @TagId";

            var parameters = new MySqlParameter[]
            {
                new MySqlParameter("@EventId", eventId),
                new MySqlParameter("@TagId", tagId)
            };

            try
            {
                _logger.LogInformation("Checking if event tag exists: EventId={EventId}, TagId={TagId}", eventId, tagId);

                var count = await _db.ExecuteQueryAsync(query, async cmd =>
                {
                    return await cmd.ExecuteScalarAsync();
                }, parameters);

                bool exists = Convert.ToInt32(count) > 0;
                _logger.LogInformation("Event tag {Exists}: EventId={EventId}, TagId={TagId}",
                    exists ? "exists" : "does not exist", eventId, tagId);

                return exists;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while checking if event tag exists: EventId={EventId}, TagId={TagId}", eventId, tagId);
                throw;
            }
        }

        // Delete event tag
        public async Task<bool> DeleteEventTagAsync(int eventId, int tagId)
        {
            const string query = "DELETE FROM EventTags WHERE EventId = @EventId AND TagId = @TagId";
            var parameters = new MySqlParameter[]
            {
                new MySqlParameter("@EventId", eventId),
                new MySqlParameter("@TagId", tagId)
            };

            try
            {
                _logger.LogInformation("Deleting event tag: EventId={EventId}, TagId={TagId}", eventId, tagId);

                int rowsAffected = await _db.ExecuteQueryAsync(query, async cmd =>
                {
                    return await cmd.ExecuteNonQueryAsync();
                }, parameters);

                if (rowsAffected == 1)
                {
                    _logger.LogInformation("Event tag deleted successfully: EventId={EventId}, TagId={TagId}", eventId, tagId);
                    return true;
                }

                _logger.LogWarning("No event tag deleted: EventId={EventId}, TagId={TagId}", eventId, tagId);
                return false;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while deleting event tag: EventId={EventId}, TagId={TagId}", eventId, tagId);
                throw;
            }
        }

        // Map event tag from database reader (Helper method)
        private EventTag MapEventTag(DbDataReader reader)
        {
            int eventIdOrdinal = reader.GetOrdinal("EventId");
            int tagIdOrdinal = reader.GetOrdinal("TagId");

            return new EventTag
            {
                EventId = reader.GetInt32(eventIdOrdinal),
                TagId = reader.GetInt32(tagIdOrdinal)
            };
        }

        // Map event tags from database reader (Helper method)
        private async Task<List<EventTag>> MapEventTags(DbDataReader reader)
        {
            var eventTags = new List<EventTag>();
            while (await reader.ReadAsync())
            {
                eventTags.Add(MapEventTag(reader));
            }
            return eventTags;
        }
    }
}
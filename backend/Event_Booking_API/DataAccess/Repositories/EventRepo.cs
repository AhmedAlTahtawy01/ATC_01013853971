using DataAccess.DataConnection;
using DataAccess.Models;
using Microsoft.Extensions.Logging;
using MySql.Data.MySqlClient;
using System.Data.Common;

namespace DataAccess.Repositories
{
    public class EventRepo
    {
        private readonly ConnectionSettings _db;
        private readonly ILogger<EventRepo> _logger;

        public EventRepo(ConnectionSettings db, ILogger<EventRepo> logger)
        {
            _db = db;
            _logger = logger;
        }

        // Create event
        public async Task<int> CreateEventAsync(Event eventData)
        {
            const string query = @"
                INSERT INTO Events (Name, Description, Category, Venue, Date, Price, ImageUrl, IsActive, CreatedBy)
                VALUES (@Name, @Description, @Category, @Venue, @Date, @Price, @ImageUrl, @IsActive, @CreatedBy);
                SELECT LAST_INSERT_ID();";

            var parameters = new MySqlParameter[]
            {
                new MySqlParameter("@Name", eventData.Name),
                new MySqlParameter("@Description", eventData.Description),
                new MySqlParameter("@Category", eventData.Category),
                new MySqlParameter("@Venue", eventData.Venue),
                new MySqlParameter("@Date", eventData.Date),
                new MySqlParameter("@Price", eventData.Price),
                new MySqlParameter("@ImageUrl", eventData.ImageUrl),
                new MySqlParameter("@IsActive", eventData.IsActive),
                new MySqlParameter("@CreatedBy", eventData.CreatedBy)
            };

            try
            {
                _logger.LogInformation("Creating event: {EventName}", eventData.Name);

                var id = await _db.ExecuteQueryAsync(query, async cmd =>
                {
                    return await cmd.ExecuteScalarAsync();
                }, parameters);

                if (id != null)
                {
                    _logger.LogInformation("Event created successfully: {EventName}", eventData.Name);
                    return Convert.ToInt32(id);
                }

                _logger.LogWarning("Failed to create event. No rows affected.");
                return 0;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while creating event: {EventName}", eventData.Name);
                throw;
            }
        }

        // Get all events (Pagination)
        public async Task<List<Event>> GetAllEventsAsync(int pageNumber = 1, int pageSize = 10)
        {
            const string query = @"
                SELECT * FROM Events
                ORDER BY Date DESC
                LIMIT @PageSize OFFSET @Offset";

            var parameters = new MySqlParameter[]
            {
                new MySqlParameter("@PageSize", pageSize),
                new MySqlParameter("@Offset", (pageNumber - 1) * pageSize)
            };

            try
            {
                _logger.LogInformation("Retrieving all events (page {PageNumber}, size {PageSize})", pageNumber, pageSize);

                return await _db.ExecuteQueryAsync(query, async cmd =>
                {
                    var reader = await cmd.ExecuteReaderAsync();
                    var eventsList = await MapEvents(reader);

                    _logger.LogInformation("Retrieved {Count} events from the database.", eventsList.Count);
                    return eventsList;
                }, parameters);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while retrieving all events");
                throw;
            }
        }

        // Get event by ID
        public async Task<Event?> GetEventByIdAsync(int eventId)
        {
            const string query = @"
                SELECT * FROM Events 
                WHERE Id = @EventId";

            var param = new MySqlParameter("@EventId", eventId);

            try
            {
                _logger.LogInformation("Retrieving event by ID: {EventId}", eventId);

                return await _db.ExecuteQueryAsync(query, async cmd =>
                {
                    using var reader = await cmd.ExecuteReaderAsync();
                    if (await reader.ReadAsync())
                    {
                        _logger.LogInformation("Event found: {EventId}", eventId);
                        return MapEvent(reader);
                    }

                    _logger.LogWarning("No event found with ID: {EventId}", eventId);
                    return null;
                }, param);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while retrieving event by ID: {EventId}", eventId);
                throw;
            }
        }

        // Get events by column (int value)
        private async Task<List<Event>> GetByIntColumnAsync(string column, int value)
        {
            var allowedColumns = new[] { "IsActive", "CreatedBy" };
            if (!allowedColumns.Contains(column))
            {
                throw new ArgumentException("Invalid column name.");
            }

            string query = $"SELECT * FROM Events WHERE {column} = @Value";
            var param = new MySqlParameter("@Value", value);

            try
            {
                _logger.LogInformation("Retrieving event by {Column}: {Value}", column, value);

                return await _db.ExecuteQueryAsync(query, async cmd =>
                {
                    using var reader = await cmd.ExecuteReaderAsync();
                    var events = await MapEvents(reader);

                    if (events.Count > 0)
                    {
                        _logger.LogInformation("Event found: {Column} = {Value}", column, value);
                        return events;
                    }

                    _logger.LogWarning("No event found with {Column}: {Value}", column, value);
                    return new List<Event>();
                }, param);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while retrieving event by {Column}: {Value}", column, value);
                throw;
            }
        }

        // Get events by isActive
        public async Task<List<Event>> GetEventsByIsActiveAsync(bool isActive)
        {
            return await GetByIntColumnAsync("IsActive", Convert.ToInt32(isActive));
        }

        // Get events by CreatedBy
        public async Task<List<Event>> GetEventsByCreatedByAsync(int createdBy)
        {
            return await GetByIntColumnAsync("CreatedBy", createdBy);
        }

        // Get events by column (string value)
        private async Task<List<Event>> GetByStringColumnAsync(string column, string value)
        {
            var allowedColumns = new[] { "Name", "Description", "Category", "Venue" };
            if (!allowedColumns.Contains(column))
            {
                throw new ArgumentException("Invalid column name.");
            }

            string query = $"SELECT * FROM Events WHERE {column} LIKE CONCAT('%', @Value, '%')";
            var param = new MySqlParameter("@Value", value);

            try
            {
                _logger.LogInformation("Retrieving event by {Column}: {Value}", column, value);

                return await _db.ExecuteQueryAsync(query, async cmd =>
                {
                    using var reader = await cmd.ExecuteReaderAsync();
                    var events = await MapEvents(reader);

                    if (events.Count > 0)
                    {
                        _logger.LogInformation("Event found: {Column} = {Value}", column, value);
                        return events;
                    }

                    _logger.LogWarning("No event found with {Column}: {Value}", column, value);
                    return new List<Event>();
                }, param);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while retrieving event by {Column}: {Value}", column, value);
                throw;
            }
        }

        // Get events by name
        public async Task<List<Event>> GetEventsByNameAsync(string name)
        {
            return await GetByStringColumnAsync("Name", name);
        }

        // Get events by description
        public async Task<List<Event>> GetEventsByDescriptionAsync(string description)
        {
            return await GetByStringColumnAsync("Description", description);
        }

        // Get events by category
        public async Task<List<Event>> GetEventsByCategoryAsync(string category)
        {
            return await GetByStringColumnAsync("Category", category);
        }

        // Get events by venue
        public async Task<List<Event>> GetEventsByVenueAsync(string venue)
        {
            return await GetByStringColumnAsync("Venue", venue);
        }

        // Get events by date
        public async Task<List<Event>> GetEventsByDateAsync(DateTime date)
        {
            const string query = "SELECT * FROM Events WHERE Date = @Date";
            var param = new MySqlParameter("@Date", date.ToString("yyyy-MM-dd"));

            try
            {
                _logger.LogInformation("Retrieving events by date: {Date}", date);

                return await _db.ExecuteQueryAsync(query, async cmd =>
                {
                    using var reader = await cmd.ExecuteReaderAsync();
                    var events = await MapEvents(reader);

                    if (events.Count > 0)
                    {
                        _logger.LogInformation("Events found: {Date}", date);
                        return events;
                    }

                    _logger.LogWarning("No events found with date: {Date}", date);
                    return new List<Event>();
                }, param);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while retrieving events by date: {Date}", date);
                throw;
            }
        }

        // Get events by price
        public async Task<List<Event>> GetEventsByPriceAsync(decimal price)
        {
            const string query = "SELECT * FROM Events WHERE Price = @Price";
            var param = new MySqlParameter("@Price", price);

            try
            {
                _logger.LogInformation("Retrieving events by price: {Price}", price);

                return await _db.ExecuteQueryAsync(query, async cmd =>
                {
                    using var reader = await cmd.ExecuteReaderAsync();
                    var events = await MapEvents(reader);

                    if (events.Count > 0)
                    {
                        _logger.LogInformation("Events found: {Price}", price);
                        return events;
                    }

                    _logger.LogWarning("No events found with price: {Price}", price);
                    return new List<Event>();
                }, param);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while retrieving events by price: {Price}", price);
                throw;
            }
        }

        // Update event
        public async Task<bool> UpdateEventAsync(Event eventData)
        {
            const string query = @"
                UPDATE Events 
                SET Name = @Name, 
                Description = @Description,     
                Category = @Category, 
                Venue = @Venue, 
                Date = @Date, 
                Price = @Price, 
                ImageUrl = @ImageUrl, 
                IsActive = @IsActive 
                WHERE Id = @EventId";

            var parameters = new MySqlParameter[]
            {
                new MySqlParameter("@Name", eventData.Name),
                new MySqlParameter("@Description", eventData.Description),
                new MySqlParameter("@Category", eventData.Category),
                new MySqlParameter("@Venue", eventData.Venue),
                new MySqlParameter("@Date", eventData.Date),
                new MySqlParameter("@Price", eventData.Price),
                new MySqlParameter("@ImageUrl", eventData.ImageUrl),
                new MySqlParameter("@IsActive", eventData.IsActive),
                new MySqlParameter("@EventId", eventData.Id)
            };

            try
            {
                _logger.LogInformation("Updating event: {EventId}", eventData.Id);

                int rowsAffected = await _db.ExecuteQueryAsync(query, async cmd =>
                {
                    return await cmd.ExecuteNonQueryAsync();
                }, parameters);

                if (rowsAffected == 1)
                {
                    _logger.LogInformation("Event updated successfully: {EventId}", eventData.Id);
                    return true;
                }

                if (rowsAffected > 1)
                {
                    _logger.LogError("Multiple events updated with the same ID: {EventId}", eventData.Id);
                    throw new InvalidOperationException("Multiple events updated with the same ID.");
                }

                _logger.LogWarning("Event update failed or no changes made for ID: {EventId}", eventData.Id);
                return false;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while updating event: {EventId}", eventData.Id);
                throw;
            }
        }

        // Delete event
        public async Task<bool> DeleteEventAsync(int eventId)
        {
            const string query = "DELETE FROM Events WHERE Id = @EventId";

            var param = new MySqlParameter("@EventId", eventId);

            try
            {
                _logger.LogInformation("Deleting event: {EventId}", eventId);

                int rowsAffected = await _db.ExecuteQueryAsync(query, async cmd =>
                {
                    return await cmd.ExecuteNonQueryAsync();
                }, param);

                if (rowsAffected == 1)
                {
                    _logger.LogInformation("Event deleted successfully: {EventId}", eventId);
                    return true;
                }

                if (rowsAffected > 1)
                {
                    _logger.LogError("Multiple events deleted with the same ID: {EventId}", eventId);
                    throw new InvalidOperationException("Multiple events deleted with the same ID.");
                }

                _logger.LogWarning("No event deleted with ID: {EventId}", eventId);
                return false;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while deleting event: {EventId}", eventId);
                throw;
            }
        }

        // Map event from database reader (Helper method)
        private Event MapEvent(DbDataReader reader)
        {
            int idOrdinal = reader.GetOrdinal("Id");
            int nameOrdinal = reader.GetOrdinal("Name");
            int descriptionOrdinal = reader.GetOrdinal("Description");
            int categoryOrdinal = reader.GetOrdinal("Category");
            int venueOrdinal = reader.GetOrdinal("Venue");
            int dateOrdinal = reader.GetOrdinal("Date");
            int priceOrdinal = reader.GetOrdinal("Price");
            int imageUrlOrdinal = reader.GetOrdinal("ImageUrl");
            int isActiveOrdinal = reader.GetOrdinal("IsActive");
            int createdByOrdinal = reader.GetOrdinal("CreatedBy");

            return new Event
            {
                Id = reader.GetInt32(idOrdinal),
                Name = reader.GetString(nameOrdinal),
                Description = reader.IsDBNull(descriptionOrdinal) ? null : reader.GetString(descriptionOrdinal),
                Category = reader.IsDBNull(categoryOrdinal) ? null : reader.GetString(categoryOrdinal),
                Venue = reader.IsDBNull(venueOrdinal) ? null : reader.GetString(venueOrdinal),
                Date = reader.GetDateTime(dateOrdinal),
                Price = reader.GetDecimal(priceOrdinal),
                ImageUrl = reader.IsDBNull(imageUrlOrdinal) ? null : reader.GetString(imageUrlOrdinal),
                IsActive = reader.GetBoolean(isActiveOrdinal),
                CreatedBy = reader.GetInt32(createdByOrdinal)
            };
        }

        // Map events from database reader (Helper method)
        private async Task<List<Event>> MapEvents(DbDataReader reader)
        {
            var events = new List<Event>();
            while (await reader.ReadAsync())
            {
                events.Add(MapEvent(reader));
            }
            return events;
        }
    }
}
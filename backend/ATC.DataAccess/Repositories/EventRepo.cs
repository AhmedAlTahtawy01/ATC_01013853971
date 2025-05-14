using ATC.DataAccess.DataConnection;
using ATC.DataAccess.Models;
using Microsoft.Extensions.Logging;
using MySql.Data.MySqlClient;
using System.Data.Common;

namespace ATC.DataAccess.Repositories
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

        public async Task<bool> CreateEventAsync(Event eventData)
        {
            const string query = @"
                INSERT INTO Events (Name, Description, Category, Venue, Date, Price, ImageUrl, IsActive, CreatedBy)
                VALUES (@Name, @Description, @Category, @Venue, @Date, @Price, @ImageUrl, @IsActive, @CreatedBy)";

            var param = new MySqlParameter[]
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

                int rowsAffected = await _db.ExecuteQueryAsync(query, async cmd =>
                {
                    return await cmd.ExecuteNonQueryAsync();
                }, param);

                if (rowsAffected == 1)
                {
                    _logger.LogInformation("Event created successfully: {EventName}", eventData.Name);
                    return true;
                }

                if (rowsAffected > 1)
                {
                    _logger.LogError("Multiple events created with the same name: {EventName}", eventData.Name);
                    throw new InvalidOperationException("Multiple events created with the same name.");
                }

                _logger.LogError("Failed to create event: {EventName}", eventData.Name);
                throw new InvalidOperationException("Failed to create event.");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while creating event: {EventName}", eventData.Name);
                throw;
            }
        }
        
        public async Task<List<Event>> GetAllEventsAsync(int pageNumber = 1, int pageSize = 10)
        {
            const string query = @"
                SELECT * FROM Events
                ORDER BY Date DESC
                LIMIT @PageSize OFFSET @Offset";

            var offset = (pageNumber - 1) * pageSize;
            var param = new MySqlParameter[]
            {
                new MySqlParameter("@PageSize", pageSize),
                new MySqlParameter("@Offset", offset)
            };

            try
            {
                _logger.LogInformation("Retrieving all events: Page {PageNumber}, PageSize {PageSize}", pageNumber, pageSize);  

                return await _db.ExecuteQueryAsync(query, async cmd =>
                {
                    using var reader = await cmd.ExecuteReaderAsync();
                    var events = new List<Event>();

                    while (await reader.ReadAsync())
                    {
                        events.Add(MapEvent(reader));
                    }

                    _logger.LogInformation("Retrieved {Count} events from the database.", events.Count);

                    return events;
                }, param);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while retrieving all events: Page {PageNumber}, PageSize {PageSize}", pageNumber, pageSize);
                throw;
            }
        }

        // Get event by ID
        public async Task<Event?> GetByIdAsync(int eventId)
        {
            const string query = @"
                SELECT * FROM Events
                WHERE Id = @EventId
                LIMIT 1";

            var param = new MySqlParameter[]
            {
                new MySqlParameter("@EventId", eventId)
            };

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

        // Get event by name
        public async Task<Event?> GetByNameAsync(string eventName)
        {
            const string query = @"
                SELECT * FROM Events
                WHERE Name = @EventName
                LIMIT 1";

            var param = new MySqlParameter("@EventName", eventName);

            try
            {
                _logger.LogInformation("Retrieving event by name: {EventName}", eventName);

                return await _db.ExecuteQueryAsync(query, async cmd =>
                {
                    using var reader = await cmd.ExecuteReaderAsync();
                    if (await reader.ReadAsync())
                    {
                        _logger.LogInformation("Event found: {EventName}", eventName);
                        return MapEvent(reader);
                    }

                    _logger.LogWarning("No event found with name: {EventName}", eventName);
                    return null;
                }, param);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while retrieving event by name: {EventName}", eventName);
                throw;
            }
        }

        // Get event by date
        public async Task<List<Event>> GetByDateAsync(DateTime eventDate)
        
        {
            const string query = "SELECT * FROM Events WHERE Date = @EventDate";
            var param = new MySqlParameter("@EventDate", eventDate);

            try
            {
                _logger.LogInformation("Retrieving events by date: {EventDate}", eventDate);

                return await _db.ExecuteQueryAsync(query, async cmd =>
                {
                    using var reader = await cmd.ExecuteReaderAsync();
                    var events = new List<Event>();

                    while (await reader.ReadAsync())
                    {
                        events.Add(MapEvent(reader));
                    }

                    _logger.LogInformation("Retrieved {Count} events from the database.", events.Count);

                    return events;
                }, param);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while retrieving events by date: {EventDate}", eventDate);
                throw;
            }
        }

        // Get event by category
        public async Task<List<Event>> GetByCategoryAsync(string category)
        {
            const string query = "SELECT * FROM Events WHERE Category = @Category";
            var param = new MySqlParameter("@Category", category);  

            try
            {
                _logger.LogInformation("Retrieving events by category: {Category}", category);

                return await _db.ExecuteQueryAsync(query, async cmd =>
                {
                    using var reader = await cmd.ExecuteReaderAsync();
                    var events = new List<Event>();

                    while (await reader.ReadAsync())
                    {
                        events.Add(MapEvent(reader));
                    }

                    _logger.LogInformation("Retrieved {Count} events from the database.", events.Count);

                    return events;
                }, param);      
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while retrieving events by category: {Category}", category);
                throw;
            }
        }

        // Get event by venue
        public async Task<List<Event>> GetByVenueAsync(string venue)
        {
            const string query = "SELECT * FROM Events WHERE Venue = @Venue";
            var param = new MySqlParameter("@Venue", venue);

            try
            {
                _logger.LogInformation("Retrieving events by venue: {Venue}", venue);

                return await _db.ExecuteQueryAsync(query, async cmd =>
                {
                    using var reader = await cmd.ExecuteReaderAsync();
                    var events = new List<Event>();

                    while (await reader.ReadAsync())
                    {
                        events.Add(MapEvent(reader));
                    }

                    _logger.LogInformation("Retrieved {Count} events from the database.", events.Count);

                    return events;
                }, param);  
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while retrieving events by venue: {Venue}", venue);
                throw;
            }
        }

        // Get event by price
        public async Task<List<Event>> GetByPriceAsync(decimal price)
        {
            const string query = "SELECT * FROM Events WHERE Price = @Price";
            var param = new MySqlParameter("@Price", price);    

            try
            {
                _logger.LogInformation("Retrieving events by price: {Price}", price);

                return await _db.ExecuteQueryAsync(query, async cmd =>
                {
                    using var reader = await cmd.ExecuteReaderAsync();
                    var events = new List<Event>();

                    while (await reader.ReadAsync())
                    {   
                        events.Add(MapEvent(reader));
                    }

                    _logger.LogInformation("Retrieved {Count} events from the database.", events.Count);

                    return events;
                }, param);  
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while retrieving events by price: {Price}", price);
                throw;
            }
        }

        // Get event by isActive
        public async Task<List<Event>> GetByIsActiveAsync(bool isActive)
        {
            const string query = "SELECT * FROM Events WHERE IsActive = @IsActive";
            var param = new MySqlParameter("@IsActive", isActive);

            try
            {
                _logger.LogInformation("Retrieving events by isActive: {IsActive}", isActive);

                return await _db.ExecuteQueryAsync(query, async cmd =>
                {
                    using var reader = await cmd.ExecuteReaderAsync();
                    var events = new List<Event>();

                    while (await reader.ReadAsync())
                    {
                        events.Add(MapEvent(reader));
                    }

                    _logger.LogInformation("Retrieved {Count} events from the database.", events.Count);

                    return events;
                }, param);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while retrieving events by isActive: {IsActive}", isActive);
                throw;
            }   
        }

        // Get event by createdBy
        public async Task<List<Event>> GetByCreatedByAsync(int createdBy)
        {
            const string query = "SELECT * FROM Events WHERE CreatedBy = @CreatedBy";
            var param = new MySqlParameter("@CreatedBy", createdBy);    

            try
            {
                _logger.LogInformation("Retrieving events by createdBy: {CreatedBy}", createdBy);

                return await _db.ExecuteQueryAsync(query, async cmd =>
                {
                    using var reader = await cmd.ExecuteReaderAsync();
                    var events = new List<Event>();

                    while (await reader.ReadAsync())
                    {   
                        events.Add(MapEvent(reader));
                    }

                    _logger.LogInformation("Retrieved {Count} events from the database.", events.Count);

                    return events;
                }, param);  
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while retrieving events by createdBy: {CreatedBy}", createdBy);
                throw;
            }
        }

        // Map event from database reader
        private Event MapEvent(DbDataReader reader)
        {
            var idOrdinal = reader.GetOrdinal("Id");
            var nameOrdinal = reader.GetOrdinal("Name");
            var descriptionOrdinal = reader.GetOrdinal("Description");
            var categoryOrdinal = reader.GetOrdinal("Category");
            var venueOrdinal = reader.GetOrdinal("Venue");
            var dateOrdinal = reader.GetOrdinal("Date");
            var priceOrdinal = reader.GetOrdinal("Price");
            var imageUrlOrdinal = reader.GetOrdinal("ImageUrl");
            var isActiveOrdinal = reader.GetOrdinal("IsActive");        
            var createdByOrdinal = reader.GetOrdinal("CreatedBy");

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

        // Update event
        public async Task<bool> UpdateEventAsync(Event eventData)
        {
            const string query = "UPDATE Events SET Name = @Name, Description = @Description, Category = @Category, Venue = @Venue, Date = @Date, Price = @Price, ImageUrl = @ImageUrl, IsActive = @IsActive, CreatedBy = @CreatedBy WHERE Id = @Id";
            var param = new MySqlParameter[]
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
                _logger.LogInformation("Updating event: {EventName}", eventData.Name);

                return await _db.ExecuteQueryAsync(query, async cmd =>
                {
                    var result = await cmd.ExecuteNonQueryAsync();
                    _logger.LogInformation("Event updated: {EventName}", eventData.Name);
                    return result > 0;
                }, param);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while updating event: {EventName}", eventData.Name);
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
                _logger.LogInformation("Deleting event by ID: {EventId}", eventId);

                return await _db.ExecuteQueryAsync(query, async cmd =>
                {
                    var result = await cmd.ExecuteNonQueryAsync();
                    _logger.LogInformation("Event deleted: {EventId}", eventId);
                    return result > 0;
                }, param);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while deleting event by ID: {EventId}", eventId);
                throw;
            }   
        }

        // Get event by date range
        public async Task<List<Event>> GetByDateRangeAsync(DateTime startDate, DateTime endDate)
        {
            const string query = "SELECT * FROM Events WHERE Date BETWEEN @StartDate AND @EndDate";
            var param = new MySqlParameter[]
            {
                new MySqlParameter("@StartDate", startDate),
                new MySqlParameter("@EndDate", endDate)
            };

            try
            {
                _logger.LogInformation("Retrieving events by date range: {StartDate} to {EndDate}", startDate, endDate);

                return await _db.ExecuteQueryAsync(query, async cmd =>
                {
                    using var reader = await cmd.ExecuteReaderAsync();
                    var events = new List<Event>();

                    while (await reader.ReadAsync())
                    {
                        events.Add(MapEvent(reader));
                    }

                    _logger.LogInformation("Retrieved {Count} events from the database.", events.Count);

                    return events;
                }, param);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while retrieving events by date range: {StartDate} to {EndDate}", startDate, endDate);
                throw;
            }
        }

        // Get event by price range
        public async Task<List<Event>> GetByPriceRangeAsync(decimal minPrice, decimal maxPrice)
        {
            const string query = "SELECT * FROM Events WHERE Price BETWEEN @MinPrice AND @MaxPrice";
            var param = new MySqlParameter[]
            {
                new MySqlParameter("@MinPrice", minPrice),
                new MySqlParameter("@MaxPrice", maxPrice)
            };

            try
            {
                _logger.LogInformation("Retrieving events by price range: {MinPrice} to {MaxPrice}", minPrice, maxPrice);

                return await _db.ExecuteQueryAsync(query, async cmd =>
                {
                    using var reader = await cmd.ExecuteReaderAsync();
                    var events = new List<Event>();

                    while (await reader.ReadAsync())
                    {
                        events.Add(MapEvent(reader));
                    }

                    _logger.LogInformation("Retrieved {Count} events from the database.", events.Count);

                    return events;
                }, param);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while retrieving events by price range: {MinPrice} to {MaxPrice}", minPrice, maxPrice);
                throw;
            }
        }
    }
}


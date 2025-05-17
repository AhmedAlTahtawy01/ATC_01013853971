using DataAccess.DataConnection;
using DataAccess.Models;
using Microsoft.Extensions.Logging;
using MySql.Data.MySqlClient;
using System.Data.Common;

namespace DataAccess.Repositories
{
    public class BookingRepo
    {
        private readonly ConnectionSettings _db;
        private readonly ILogger<BookingRepo> _logger;

        public BookingRepo(ConnectionSettings db, ILogger<BookingRepo> logger)
        {
            _db = db;
            _logger = logger;
        }

        // Create booking
        public async Task<int> CreateBookingAsync(Booking booking)
        {
            const string query = @"
                INSERT INTO Bookings (UserId, EventId, BookedAt)
                VALUES (@UserId, @EventId, @BookedAt);
                SELECT LAST_INSERT_ID();";

            var parameters = new MySqlParameter[]
            {
                new MySqlParameter("@UserId", booking.UserId),
                new MySqlParameter("@EventId", booking.EventId),
                new MySqlParameter("@BookedAt", booking.BookedAt)
            };

            try
            {
                _logger.LogInformation("Creating booking for User: {UserId}, Event: {EventId}", booking.UserId, booking.EventId);

                var id = await _db.ExecuteQueryAsync(query, async cmd =>
                {
                    return await cmd.ExecuteScalarAsync();
                }, parameters);

                if (id != null)
                {
                    _logger.LogInformation("Booking created successfully: {BookingId}", id);
                    return Convert.ToInt32(id);
                }

                _logger.LogWarning("Failed to create booking. No rows affected.");
                return 0;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while creating booking for User: {UserId}, Event: {EventId}", booking.UserId, booking.EventId);
                throw;
            }
        }

        // Get all bookings (Pagination)
        public async Task<List<Booking>> GetAllBookingsAsync(int pageNumber = 1, int pageSize = 10)
        {
            const string query = @"
                SELECT * FROM Bookings
                ORDER BY BookedAt DESC
                LIMIT @PageSize OFFSET @Offset";

            var parameters = new MySqlParameter[]
            {
                new MySqlParameter("@PageSize", pageSize),
                new MySqlParameter("@Offset", (pageNumber - 1) * pageSize)
            };

            try
            {
                _logger.LogInformation("Retrieving all bookings (page {PageNumber}, size {PageSize})", pageNumber, pageSize);

                return await _db.ExecuteQueryAsync(query, async cmd =>
                {
                    var reader = await cmd.ExecuteReaderAsync();
                    var bookingsList = await MapBookings(reader);

                    _logger.LogInformation("Retrieved {Count} bookings from the database.", bookingsList.Count);
                    return bookingsList;
                }, parameters);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while retrieving all bookings");
                throw;
            }
        }

        // Count all bookings in the system
        public async Task<int> CountBookingsAsync()
        {
            const string query = "SELECT COUNT(*) FROM Bookings";

            try
            {
                _logger.LogInformation("Counting all bookings in the system");

                var bookingsCount = await _db.ExecuteQueryAsync(query, async cmd =>
                {
                    return await cmd.ExecuteScalarAsync();
                });

                if (bookingsCount != null)
                {
                    _logger.LogInformation("Total bookings in the system: {Count}", bookingsCount);
                    return Convert.ToInt32(bookingsCount);
                }

                _logger.LogWarning("No bookings found in the system");
                return 0;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while counting all bookings in the system");
                throw;
            }
        }

        // Get booking by ID
        public async Task<Booking?> GetBookingByIdAsync(int bookingId)
        {
            const string query = @"
                SELECT * FROM Bookings 
                WHERE Id = @BookingId";

            var param = new MySqlParameter("@BookingId", bookingId);

            try
            {
                _logger.LogInformation("Retrieving booking by ID: {BookingId}", bookingId);

                return await _db.ExecuteQueryAsync(query, async cmd =>
                {
                    using var reader = await cmd.ExecuteReaderAsync();
                    if (await reader.ReadAsync())
                    {
                        _logger.LogInformation("Booking found: {BookingId}", bookingId);
                        return MapBooking(reader);
                    }

                    _logger.LogWarning("No booking found with ID: {BookingId}", bookingId);
                    return null;
                }, param);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while retrieving booking by ID: {BookingId}", bookingId);
                throw;
            }
        }

        // Get bookings by UserId
        public async Task<List<Booking>> GetBookingsByUserIdAsync(int userId)
        {
            const string query = "SELECT * FROM Bookings WHERE UserId = @UserId";
            var param = new MySqlParameter("@UserId", userId);

            try
            {
                _logger.LogInformation("Retrieving bookings by User ID: {UserId}", userId);

                return await _db.ExecuteQueryAsync(query, async cmd =>
                {
                    using var reader = await cmd.ExecuteReaderAsync();
                    var bookings = await MapBookings(reader);

                    if (bookings.Count > 0)
                    {
                        _logger.LogInformation("Found {Count} bookings for User ID: {UserId}", bookings.Count, userId);
                        return bookings;
                    }

                    _logger.LogWarning("No bookings found for User ID: {UserId}", userId);
                    return new List<Booking>();
                }, param);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while retrieving bookings by User ID: {UserId}", userId);
                throw;
            }
        }

        // Get bookings by EventId
        public async Task<List<Booking>> GetBookingsByEventIdAsync(int eventId)
        {
            const string query = "SELECT * FROM Bookings WHERE EventId = @EventId";
            var param = new MySqlParameter("@EventId", eventId);

            try
            {
                _logger.LogInformation("Retrieving bookings by Event ID: {EventId}", eventId);

                return await _db.ExecuteQueryAsync(query, async cmd =>
                {
                    using var reader = await cmd.ExecuteReaderAsync();
                    var bookings = await MapBookings(reader);

                    if (bookings.Count > 0)
                    {
                        _logger.LogInformation("Found {Count} bookings for Event ID: {EventId}", bookings.Count, eventId);
                        return bookings;
                    }

                    _logger.LogWarning("No bookings found for Event ID: {EventId}", eventId);
                    return new List<Booking>();
                }, param);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while retrieving bookings by Event ID: {EventId}", eventId);
                throw;
            }
        }

        // Checks if booking exists
        public async Task<bool> CheckBookingExistsAsync(int userId, int eventId)
        {
            const string query = "SELECT COUNT(*) FROM Bookings WHERE UserId = @UserId AND EventId = @EventId";
            var param = new MySqlParameter[]
            {
                new MySqlParameter("@UserId", userId),
                new MySqlParameter("@EventId", eventId)
            };

            try
            {
                _logger.LogInformation("Checking if booking exists for User ID: {UserId} and Event ID: {EventId}", userId, eventId);

                return await _db.ExecuteQueryAsync(query, async cmd =>
                {
                    using var reader = await cmd.ExecuteReaderAsync();
                    if (await reader.ReadAsync())
                    {
                        _logger.LogInformation("Booking found: {BookingId}", reader.GetInt32(0));
                        return reader.GetInt32(0) > 0;
                    }

                    _logger.LogWarning("No booking found for User ID: {UserId} and Event ID: {EventId}", userId, eventId);
                    return false;
                }, param);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while checking if booking exists for User ID: {UserId} and Event ID: {EventId}", userId, eventId);
                throw;
            }
        }

        // Delete booking
        public async Task<bool> DeleteBookingAsync(int bookingId)
        {
            const string query = "DELETE FROM Bookings WHERE Id = @BookingId";
            var param = new MySqlParameter("@BookingId", bookingId);

            try
            {
                _logger.LogInformation("Deleting booking: {BookingId}", bookingId);

                int rowsAffected = await _db.ExecuteQueryAsync(query, async cmd =>
                {
                    return await cmd.ExecuteNonQueryAsync();
                }, param);

                if (rowsAffected == 1)
                {
                    _logger.LogInformation("Booking deleted successfully: {BookingId}", bookingId);
                    return true;
                }

                if (rowsAffected > 1)
                {
                    _logger.LogError("Multiple bookings deleted with the same ID: {BookingId}", bookingId);
                    throw new InvalidOperationException("Multiple bookings deleted with the same ID.");
                }

                _logger.LogWarning("No booking deleted with ID: {BookingId}", bookingId);
                return false;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while deleting booking: {BookingId}", bookingId);
                throw;
            }
        }

        // Map booking from database reader (Helper method)
        private Booking MapBooking(DbDataReader reader)
        {
            int idOrdinal = reader.GetOrdinal("Id");
            int userIdOrdinal = reader.GetOrdinal("UserId");
            int eventIdOrdinal = reader.GetOrdinal("EventId");
            int bookedAtOrdinal = reader.GetOrdinal("BookedAt");

            return new Booking
            {
                Id = reader.GetInt32(idOrdinal),
                UserId = reader.GetInt32(userIdOrdinal),
                EventId = reader.GetInt32(eventIdOrdinal),
                BookedAt = reader.GetDateTime(bookedAtOrdinal)
            };
        }

        // Map bookings from database reader (Helper method)
        private async Task<List<Booking>> MapBookings(DbDataReader reader)
        {
            var bookings = new List<Booking>();
            while (await reader.ReadAsync())
            {
                bookings.Add(MapBooking(reader));
            }
            return bookings;
        }
    }
}
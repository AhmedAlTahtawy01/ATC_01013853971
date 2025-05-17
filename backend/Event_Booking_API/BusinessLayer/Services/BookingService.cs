using DataAccess.Repositories;
using DataAccess.Models;
using Microsoft.Extensions.Logging;
using BusinessLogic.Helpers;

namespace BusinessLogic.Services
{
    public class BookingService
    {
        private readonly BookingRepo _bookingRepo;
        private readonly EventRepo _eventRepo;
        private readonly ILogger<BookingService> _logger;
        private readonly Shared _shared;

        public BookingService(BookingRepo bookingRepo, EventRepo eventRepo, ILogger<BookingService> logger, Shared shared)
        {
            _bookingRepo = bookingRepo;
            _eventRepo = eventRepo;
            _logger = logger;
            _shared = shared;
        }

        // Validate booking before creating (Helper method)
        private async Task ValidateBooking(Booking booking)
        {
            if (booking == null)
            {
                _logger.LogError("Booking data is null");
                throw new ArgumentNullException(nameof(booking));
            }

            if (booking.UserId <= 0)
            {
                _logger.LogError("Invalid User ID");
                throw new ArgumentException("User ID must be greater than 0");
            }

            if (booking.EventId <= 0)
            {
                _logger.LogError("Invalid Event ID");
                throw new ArgumentException("Event ID must be greater than 0");
            }

            // Check if user exists
            if (!await _shared.IsUserExists(booking.UserId))
            {
                _logger.LogError("User does not exist");
                throw new KeyNotFoundException("User does not exist");
            }

            // Check if event exists and is active
            var eventData = await _eventRepo.GetEventByIdAsync(booking.EventId);
            if (eventData == null)
            {
                _logger.LogError("Event does not exist");
                throw new KeyNotFoundException("Event does not exist");
            }

            if (!eventData.IsActive)
            {
                _logger.LogError("Event is not active");
                throw new InvalidOperationException("Event is not active");
            }
        }

        // Create booking
        public async Task<int> CreateBookingAsync(Booking booking)
        {
            await ValidateBooking(booking);

            // Check if booking already exists
            if (await _bookingRepo.CheckBookingExistsAsync(booking.UserId, booking.EventId))
            {
                _logger.LogWarning("Booking already exists for User ID: {UserId} and Event ID: {EventId}", booking.UserId, booking.EventId);
                throw new InvalidOperationException("Booking already exists for this user and event");
            }

            try
            {
                _logger.LogInformation("Creating booking: {Booking}", booking);
                int newBookingId = await _bookingRepo.CreateBookingAsync(booking);

                if (newBookingId > 0)
                {
                    _logger.LogInformation("Booking created successfully: {Booking}", booking);
                    return newBookingId;
                }

                _logger.LogWarning("Failed to create booking: {Booking}", booking);
                return 0;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating booking");
                throw;
            }
        }

        // Get all bookings with pagination
        public async Task<List<Booking>> GetAllBookingsAsync(int pageNumber = 1, int pageSize = 10)
        {
            if (pageNumber <= 0 || pageSize <= 0)
            {
                _logger.LogError("Invalid page number or page size");
                throw new ArgumentException("Page number and page size must be greater than 0");
            }

            try
            {
                _logger.LogInformation("Retrieving all bookings with pagination");
                return await _bookingRepo.GetAllBookingsAsync(pageNumber, pageSize);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving all bookings with pagination");
                throw;
            }
        }

        // Get total count of bookings
        public async Task<int> GetBookingCountAsync()
        {
            try
            {
                _logger.LogInformation("Retrieving total booking count");
                return await _bookingRepo.CountBookingsAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while retrieving booking count");
                throw;
            }
        }

        // Get booking by ID
        public async Task<Booking?> GetBookingByIdAsync(int bookingId)
        {
            if (bookingId <= 0)
            {
                _logger.LogError("Invalid booking ID");
                throw new ArgumentException("Booking ID must be greater than 0");
            }

            try
            {
                _logger.LogInformation("Retrieving booking by ID: {BookingId}", bookingId);
                var booking = await _bookingRepo.GetBookingByIdAsync(bookingId);

                if (booking == null)
                {
                    _logger.LogError("Booking not found with ID: {BookingId}", bookingId);
                    throw new KeyNotFoundException("Booking not found");
                }

                return booking;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving booking by ID: {BookingId}", bookingId);
                throw;
            }
        }

        // Get bookings by UserId
        public async Task<List<Booking>> GetBookingsByUserIdAsync(int userId)
        {
            if (userId <= 0)
            {
                _logger.LogError("Invalid User ID");
                throw new ArgumentException("User ID must be greater than 0");
            }

            if (!await _shared.IsUserExists(userId))
            {
                _logger.LogError("User does not exist");
                throw new KeyNotFoundException("User does not exist");
            }

            try
            {
                _logger.LogInformation("Retrieving bookings by User ID: {UserId}", userId);
                return await _bookingRepo.GetBookingsByUserIdAsync(userId);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving bookings by User ID: {UserId}", userId);
                throw;
            }
        }

        // Get bookings by EventId
        public async Task<List<Booking>> GetBookingsByEventIdAsync(int eventId)
        {
            if (eventId <= 0)
            {
                _logger.LogError("Invalid Event ID");
                throw new ArgumentException("Event ID must be greater than 0");
            }

            var eventData = await _eventRepo.GetEventByIdAsync(eventId);
            if (eventData == null)
            {
                _logger.LogError("Event does not exist");
                throw new KeyNotFoundException("Event does not exist");
            }

            try
            {
                _logger.LogInformation("Retrieving bookings by Event ID: {EventId}", eventId);
                return await _bookingRepo.GetBookingsByEventIdAsync(eventId);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving bookings by Event ID: {EventId}", eventId);
                throw;
            }
        }

        // Delete booking
        public async Task<bool> DeleteBookingAsync(int bookingId)
        {
            if (bookingId <= 0)
            {
                _logger.LogError("Invalid booking ID: {BookingId}", bookingId);
                throw new ArgumentException("Booking ID must be greater than 0");
            }

            try
            {
                _logger.LogInformation("Deleting booking by ID: {BookingId}", bookingId);
                return await _bookingRepo.DeleteBookingAsync(bookingId);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting booking by ID: {BookingId}", bookingId);
                throw;
            }
        }
    }
}
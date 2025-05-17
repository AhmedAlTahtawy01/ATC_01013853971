using Microsoft.AspNetCore.Mvc;
using BusinessLogic.Services;
using DataAccess.Models;
using Microsoft.AspNetCore.Authorization;

namespace EventBooking.Controllers
{
    [Authorize]
    [Route("api/[controller]")]
    [ApiController]
    public class BookingController : ControllerBase
    {
        private readonly BookingService _bookingService;
        private readonly ILogger<BookingController> _logger;

        public BookingController(BookingService bookingService, ILogger<BookingController> logger)
        {
            _bookingService = bookingService;
            _logger = logger;
        }

        [HttpPost]
        public async Task<IActionResult> CreateBooking([FromBody] Booking booking)
        {
            if (!ModelState.IsValid)
            {
                _logger.LogError("Invalid booking data");
                return BadRequest(new { message = "Invalid booking data", errors = ModelState });
            }

            try
            {
                _logger.LogInformation("Creating booking: {Booking}", booking);
                int newBookingId = await _bookingService.CreateBookingAsync(booking);

                if (newBookingId > 0)
                {
                    _logger.LogInformation("Booking created successfully: {Booking}", booking);
                    return Ok(new { message = "Booking created successfully", bookingId = newBookingId });
                }

                _logger.LogError("Failed to create booking: {Booking}", booking);
                return BadRequest(new { message = "Failed to create booking" });
            }
            catch (ArgumentException ex)
            {
                _logger.LogError(ex, "Error creating booking: {Booking}", booking);
                return BadRequest(new { message = ex.Message });
            }
            catch (KeyNotFoundException ex)
            {
                _logger.LogError(ex, "Error creating booking: {Booking}", booking);
                return NotFound(new { message = ex.Message });
            }
            catch (InvalidOperationException ex)
            {
                _logger.LogError(ex, "Error creating booking: {Booking}", booking);
                return BadRequest(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating booking: {Booking}", booking);
                return StatusCode(500, new { message = "Internal server error" });
            }
        }

        [Authorize(Roles = "1")]
        [HttpGet]
        public async Task<IActionResult> GetAllBookings([FromQuery] int pageNumber = 1, [FromQuery] int pageSize = 10)
        {
            try
            {
                _logger.LogInformation("Retrieving all bookings with pagination");
                var bookings = await _bookingService.GetAllBookingsAsync(pageNumber, pageSize);

                if (bookings.Count == 0)
                {
                    _logger.LogWarning("No bookings found");
                    return NotFound(new { message = "No bookings found" });
                }

                return Ok(new { message = "Bookings retrieved successfully", bookings });
            }
            catch (ArgumentException ex)
            {
                _logger.LogError(ex, "Error retrieving all bookings with pagination");
                return BadRequest(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving all bookings with pagination");
                return StatusCode(500, new { message = "Internal server error" });
            }
        }

        [HttpGet("{bookingId}")]
        public async Task<IActionResult> GetById([FromRoute] int bookingId)
        {
            try
            {
                _logger.LogInformation("Retrieving booking by ID: {BookingId}", bookingId);
                var booking = await _bookingService.GetBookingByIdAsync(bookingId);
                return Ok(new { message = "Booking retrieved successfully", booking });
            }
            catch (ArgumentException ex)
            {
                _logger.LogError(ex, "Error retrieving booking by ID: {BookingId}", bookingId);
                return BadRequest(new { message = ex.Message });
            }
            catch (KeyNotFoundException ex)
            {
                _logger.LogError(ex, "Error retrieving booking by ID: {BookingId}", bookingId);
                return NotFound(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving booking by ID: {BookingId}", bookingId);
                return StatusCode(500, new { message = "Internal server error" });
            }
        }

        [HttpGet("user/{userId}")]
        public async Task<IActionResult> GetBookingsByUserId([FromRoute] int userId)
        {
            try
            {
                _logger.LogInformation("Retrieving bookings by User ID: {UserId}", userId);
                var bookings = await _bookingService.GetBookingsByUserIdAsync(userId);

                if (bookings.Count == 0)
                {
                    _logger.LogWarning("No bookings found for User ID: {UserId}", userId);
                    return NotFound(new { message = "No bookings found for the given User ID" });
                }

                return Ok(new { message = "Bookings retrieved successfully", bookings });
            }
            catch (ArgumentException ex)
            {
                _logger.LogError(ex, "Error retrieving bookings by User ID: {UserId}", userId);
                return BadRequest(new { message = ex.Message });
            }
            catch (KeyNotFoundException ex)
            {
                _logger.LogError(ex, "Error retrieving bookings by User ID: {UserId}", userId);
                return NotFound(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving bookings by User ID: {UserId}", userId);
                return StatusCode(500, new { message = "Internal server error" });
            }
        }

        [Authorize(Roles = "1")]
        [HttpGet("event/{eventId}")]
        public async Task<IActionResult> GetBookingsByEventId([FromRoute] int eventId)
        {
            try
            {
                _logger.LogInformation("Retrieving bookings by Event ID: {EventId}", eventId);
                var bookings = await _bookingService.GetBookingsByEventIdAsync(eventId);

                if (bookings.Count == 0)
                {
                    _logger.LogWarning("No bookings found for Event ID: {EventId}", eventId);
                    return NotFound(new { message = "No bookings found for the given Event ID" });
                }

                return Ok(new { message = "Bookings retrieved successfully", bookings });
            }
            catch (ArgumentException ex)
            {
                _logger.LogError(ex, "Error retrieving bookings by Event ID: {EventId}", eventId);
                return BadRequest(new { message = ex.Message });
            }
            catch (KeyNotFoundException ex)
            {
                _logger.LogError(ex, "Error retrieving bookings by Event ID: {EventId}", eventId);
                return NotFound(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving bookings by Event ID: {EventId}", eventId);
                return StatusCode(500, new { message = "Internal server error" });
            }
        }

        [HttpDelete("{bookingId}")]
        public async Task<IActionResult> DeleteBooking([FromRoute] int bookingId)
        {
            try
            {
                _logger.LogInformation("Deleting booking by ID: {BookingId}", bookingId);
                bool deleted = await _bookingService.DeleteBookingAsync(bookingId);

                if (deleted)
                {
                    _logger.LogInformation("Booking deleted successfully: {BookingId}", bookingId);
                    return Ok(new { message = "Booking deleted successfully" });
                }

                _logger.LogError("Failed to delete booking: {BookingId}", bookingId);
                return BadRequest(new { message = "Failed to delete booking" });
            }
            catch (ArgumentException ex)
            {
                _logger.LogError(ex, "Error deleting booking: {BookingId}", bookingId);
                return BadRequest(new { message = ex.Message });
            }
            catch (KeyNotFoundException ex)
            {
                _logger.LogError(ex, "Error deleting booking: {BookingId}", bookingId);
                return NotFound(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting booking: {BookingId}", bookingId);
                return StatusCode(500, new { message = "Internal server error" });
            }
        }
    }
}
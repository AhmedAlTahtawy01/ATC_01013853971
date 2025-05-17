using Microsoft.AspNetCore.Mvc;
using BusinessLogic.Services;
using DataAccess.Models;
using Microsoft.AspNetCore.Authorization;

namespace EventBooking.Controllers
{
    [Authorize]
    [Route("api/[controller]")]
    [ApiController]
    public class EventController : ControllerBase
    {
        private readonly EventService _eventService;
        private readonly ILogger<EventController> _logger;

        public EventController(EventService eventService, ILogger<EventController> logger)
        {
            _eventService = eventService;
            _logger = logger;
        }

        [HttpPost]
        public async Task<IActionResult> CreateEvent([FromBody] Event eventData)
        {
            if (!ModelState.IsValid)
            {
                _logger.LogError("Invalid event data");
                return BadRequest(new { message = "Invalid event data", errors = ModelState });
            }

            try
            {
                _logger.LogInformation("Creating event: {Event}", eventData);
                int newEventId = await _eventService.CreateEventAsync(eventData);

                if (newEventId > 0)
                {
                    _logger.LogInformation("Event created successfully: {Event}", eventData);
                    return Ok(new { message = "Event created successfully", eventId = newEventId });
                }

                _logger.LogError("Failed to create event: {Event}", eventData);
                return BadRequest(new { message = "Failed to create event" });
            }
            catch (ArgumentException ex)
            {
                _logger.LogError(ex, "Error creating event: {Event}", eventData);
                return BadRequest(new { message = ex.Message });
            }
            catch (KeyNotFoundException ex)
            {
                _logger.LogError(ex, "Error creating event: {Event}", eventData);
                return NotFound(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating event: {Event}", eventData);
                return StatusCode(500, new { message = "Internal server error" });
            }
        }

        [HttpGet]
        public async Task<IActionResult> GetAllEvents([FromQuery] int pageNumber = 1, [FromQuery] int pageSize = 10)
        {
            try
            {
                _logger.LogInformation("Retrieving all events with pagination");
                var events = await _eventService.GetAllEventsAsync(pageNumber, pageSize);

                if (events.Count == 0)
                {
                    _logger.LogWarning("No events found");
                    return NotFound(new { message = "No events found" });
                }

                return Ok(new { message = "Events retrieved successfully", events });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving all events with pagination");
                return StatusCode(500, new { message = "Internal server error" });
            }
        }

        [Authorize(Roles = "1")]
        [HttpGet("count")]
        public async Task<IActionResult> GetEventCount()
        {
            try
            {
                _logger.LogInformation("Retrieving total event count");
                var count = await _eventService.GetEventCountAsync();
                return Ok(new { message = "Event count retrieved successfully", count });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving event count");
                return StatusCode(500, new { message = "Internal server error" });
            }
        }

        [HttpGet("{eventId}")]
        public async Task<IActionResult> GetById([FromRoute] int eventId)
        {
            try
            {
                _logger.LogInformation("Retrieving event by ID: {EventId}", eventId);
                var eventData = await _eventService.GetEventByIdAsync(eventId);
                return Ok(new { message = "Event retrieved successfully", eventData });
            }
            catch (ArgumentException ex)
            {
                _logger.LogError(ex, "Error retrieving event by ID: {EventId}", eventId);
                return BadRequest(new { message = ex.Message });
            }
            catch (KeyNotFoundException ex)
            {
                _logger.LogError(ex, "Error retrieving event by ID: {EventId}", eventId);
                return NotFound(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving event by ID: {EventId}", eventId);
                return StatusCode(500, new { message = "Internal server error" });
            }
        }

        [HttpGet("Active-status")]
        public async Task<IActionResult> GetEventsByIsActive([FromQuery] bool ActiveStatus)
        {
            try
            {
                _logger.LogInformation("Retrieving events by Active Status: {ActiveStatus}", ActiveStatus);
                var events = await _eventService.GetEventsByIsActiveAsync(ActiveStatus);

                if (events.Count == 0)
                {
                    _logger.LogWarning("No events found with Active Status: {ActiveStatus}", ActiveStatus);
                    return NotFound(new { message = "No events found with the given Active Status" });
                }

                return Ok(new { message = "Events retrieved successfully", events });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving events by Active Status: {ActiveStatus}", ActiveStatus);
                return StatusCode(500, new { message = "Internal server error" });
            }
        }

        [HttpGet("User-ID")]
        public async Task<IActionResult> GetEventsByCreatedBy([FromQuery] int createdBy)
        {
            try
            {
                _logger.LogInformation("Retrieving events by User ID: {CreatedBy}", createdBy);
                var events = await _eventService.GetEventsByCreatedByAsync(createdBy);

                if (events.Count == 0)
                {
                    _logger.LogWarning("No events found with User ID: {CreatedBy}", createdBy);
                    return NotFound(new { message = "No events found with the given User ID" });
                }

                return Ok(new { message = "Events retrieved successfully", events });
            }
            catch (ArgumentException ex)
            {
                _logger.LogError(ex, "Error retrieving events by User ID: {CreatedBy}", createdBy);
                return BadRequest(new { message = ex.Message });
            }
            catch (KeyNotFoundException ex)
            {
                _logger.LogError(ex, "Error retrieving events by User ID: {CreatedBy}", createdBy);
                return NotFound(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving events by User ID: {CreatedBy}", createdBy);
                return StatusCode(500, new { message = "Internal server error" });
            }
        }

        [HttpGet("name")]
        public async Task<IActionResult> GetEventsByName([FromQuery] string name)
        {
            try
            {
                _logger.LogInformation("Retrieving events by name: {Name}", name);
                var events = await _eventService.GetEventsByNameAsync(name);

                if (events.Count == 0)
                {
                    _logger.LogWarning("No events found with name: {Name}", name);
                    return NotFound(new { message = "No events found with the given name" });
                }

                return Ok(new { message = "Events retrieved successfully", events });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving events by name: {Name}", name);
                return StatusCode(500, new { message = "Internal server error" });
            }
        }

        [HttpGet("description")]
        public async Task<IActionResult> GetEventsByDescription([FromQuery] string description)
        {
            try
            {
                _logger.LogInformation("Retrieving events by description: {Description}", description);
                var events = await _eventService.GetEventsByDescriptionAsync(description);

                if (events.Count == 0)
                {
                    _logger.LogWarning("No events found with description: {Description}", description);
                    return NotFound(new { message = "No events found with the given description" });
                }

                return Ok(new { message = "Events retrieved successfully", events });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving events by description: {Description}", description);
                return StatusCode(500, new { message = "Internal server error" });
            }
        }

        [HttpGet("category")]
        public async Task<IActionResult> GetEventsByCategory([FromQuery] string category)
        {
            try
            {
                _logger.LogInformation("Retrieving events by category: {Category}", category);
                var events = await _eventService.GetEventsByCategoryAsync(category);

                if (events.Count == 0)
                {
                    _logger.LogWarning("No events found with category: {Category}", category);
                    return NotFound(new { message = "No events found with the given category" });
                }

                return Ok(new { message = "Events retrieved successfully", events });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving events by category: {Category}", category);
                return StatusCode(500, new { message = "Internal server error" });
            }
        }

        [HttpGet("venue")]
        public async Task<IActionResult> GetEventsByVenue([FromQuery] string venue)
        {
            try
            {
                _logger.LogInformation("Retrieving events by venue: {Venue}", venue);
                var events = await _eventService.GetEventsByVenueAsync(venue);

                if (events.Count == 0)
                {
                    _logger.LogWarning("No events found with venue: {Venue}", venue);
                    return NotFound(new { message = "No events found with the given venue" });
                }

                return Ok(new { message = "Events retrieved successfully", events });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving events by venue: {Venue}", venue);
                return StatusCode(500, new { message = "Internal server error" });
            }
        }

        [HttpGet("date")]
        public async Task<IActionResult> GetEventsByDate([FromQuery] DateTime date)
        {
            try
            {
                _logger.LogInformation("Retrieving events by date: {Date}", date);
                var events = await _eventService.GetEventsByDateAsync(date);

                if (events.Count == 0)
                {
                    _logger.LogWarning("No events found with date: {Date}", date);
                    return NotFound(new { message = "No events found with the given date" });
                }

                return Ok(new { message = "Events retrieved successfully", events });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving events by date: {Date}", date);
                return StatusCode(500, new { message = "Internal server error" });
            }
        }

        [HttpGet("price")]
        public async Task<IActionResult> GetEventsByPrice([FromQuery] decimal price)
        {
            try
            {
                _logger.LogInformation("Retrieving events by price: {Price}", price);
                var events = await _eventService.GetEventsByPriceAsync(price);

                if (events.Count == 0)
                {
                    _logger.LogWarning("No events found with price: {Price}", price);
                    return NotFound(new { message = "No events found with the given price" });
                }

                return Ok(new { message = "Events retrieved successfully", events });
            }
            catch (ArgumentException ex)
            {
                _logger.LogError(ex, "Error retrieving events by price: {Price}", price);
                return BadRequest(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving events by price: {Price}", price);
                return StatusCode(500, new { message = "Internal server error" });
            }
        }

        [HttpPut("{eventId}")]
        public async Task<IActionResult> UpdateEvent([FromRoute] int eventId, [FromBody] Event eventData)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    _logger.LogError("Invalid event data");
                    return BadRequest(new { message = "Invalid event data", errors = ModelState });
                }

                if (eventId != eventData.Id)
                {
                    _logger.LogError("Event ID in route does not match body: Route={EventId}, Body={BodyId}", eventId, eventData.Id);
                    return BadRequest(new { message = "Event ID mismatch." });
                }

                _logger.LogInformation("Updating event: {Event}", eventData);
                bool updated = await _eventService.UpdateEventAsync(eventData);

                if (updated)
                {
                    _logger.LogInformation("Event updated successfully: {Event}", eventData);
                    return Ok(new { message = "Event updated successfully" });
                }

                _logger.LogError("Failed to update event: {Event}", eventData);
                return BadRequest(new { message = "Failed to update event" });
            }
            catch (ArgumentException ex)
            {
                _logger.LogError(ex, "Error updating event: {Event}", eventData);
                return BadRequest(new { message = ex.Message });
            }
            catch (KeyNotFoundException ex)
            {
                _logger.LogError(ex, "Error updating event: {Event}", eventData);
                return NotFound(new { message = ex.Message });
            }
            catch (InvalidOperationException ex)
            {
                _logger.LogError(ex, "Error updating event: {Event}", eventData);
                return BadRequest(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating event: {Event}", eventData);
                return StatusCode(500, new { message = "Internal server error" });
            }
        }

        [HttpDelete("{eventId}")]
        public async Task<IActionResult> DeleteEvent([FromRoute] int eventId)
        {
            try
            {
                _logger.LogInformation("Deleting event by ID: {EventId}", eventId);
                bool deleted = await _eventService.DeleteEventAsync(eventId);

                if (deleted)
                {
                    _logger.LogInformation("Event deleted successfully: {EventId}", eventId);
                    return Ok(new { message = "Event deleted successfully" });
                }

                _logger.LogError("Failed to delete event: {EventId}", eventId);
                return BadRequest(new { message = "Failed to delete event" });
            }
            catch (ArgumentException ex)
            {
                _logger.LogError(ex, "Error deleting event by ID: {EventId}", eventId);
                return BadRequest(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting event by ID: {EventId}", eventId);
                return StatusCode(500, new { message = "Internal server error" });
            }
        }
    }
}
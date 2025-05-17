using Microsoft.AspNetCore.Mvc;
using BusinessLogic.Services;
using DataAccess.Models;
using Microsoft.AspNetCore.Authorization;

namespace EventBooking.Controllers
{
    [Authorize]
    [ApiController]
    [Route("api/[controller]")]
    public class EventTagController : ControllerBase
    {
        private readonly EventTagService _eventTagService;
        private readonly ILogger<EventTagController> _logger;

        public EventTagController(EventTagService eventTagService, ILogger<EventTagController> logger)
        {
            _eventTagService = eventTagService;
            _logger = logger;
        }

        [HttpPost]
        public async Task<IActionResult> CreateEventTag([FromBody] EventTag eventTag)
        {
            try
            {
                _logger.LogInformation("Creating new event tag");
                bool success = await _eventTagService.CreateEventTagAsync(eventTag);
                return Ok(new { Message = "Event tag created successfully" });
            }
            catch (ArgumentNullException ex)
            {
                _logger.LogError(ex, "Invalid event tag data provided");
                return BadRequest(new { Message = ex.Message });
            }
            catch (ArgumentException ex)
            {
                _logger.LogError(ex, "Invalid event tag data provided");
                return BadRequest(new { Message = ex.Message });
            }
            catch (KeyNotFoundException ex)
            {
                _logger.LogError(ex, "Event or tag not found");
                return NotFound(new { Message = ex.Message });
            }
            catch (InvalidOperationException ex)
            {
                _logger.LogError(ex, "Event tag creation failed due to business rule violation");
                return Conflict(new { Message = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating event tag");
                return StatusCode(500, new { Message = "An error occurred while creating the event tag" });
            }
        }

        [HttpGet]
        public async Task<IActionResult> GetAllEventTags()
        {
            try
            {
                _logger.LogInformation("Retrieving all event tags");
                var eventTags = await _eventTagService.GetAllEventTagsAsync();
                return Ok(eventTags);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving event tags");
                return StatusCode(500, new { Message = "An error occurred while retrieving event tags" });
            }
        }

        [HttpGet("event/{eventId}")]
        public async Task<IActionResult> GetEventTagsByEventId(int eventId)
        {
            try
            {
                _logger.LogInformation("Retrieving event tags by EventId: {EventId}", eventId);
                var eventTags = await _eventTagService.GetEventTagsByEventIdAsync(eventId);
                return Ok(eventTags);
            }
            catch (ArgumentException ex)
            {
                _logger.LogError(ex, "Invalid event ID provided");
                return BadRequest(new { Message = ex.Message });
            }
            catch (KeyNotFoundException ex)
            {
                _logger.LogError(ex, "Event not found");
                return NotFound(new { Message = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving event tags");
                return StatusCode(500, new { Message = "An error occurred while retrieving event tags" });
            }
        }

        [HttpGet("tag/{tagId}")]
        public async Task<IActionResult> GetEventTagsByTagId(int tagId)
        {
            try
            {
                _logger.LogInformation("Retrieving event tags by TagId: {TagId}", tagId);
                var eventTags = await _eventTagService.GetEventTagsByTagIdAsync(tagId);
                return Ok(eventTags);
            }
            catch (ArgumentException ex)
            {
                _logger.LogError(ex, "Invalid tag ID provided");
                return BadRequest(new { Message = ex.Message });
            }
            catch (KeyNotFoundException ex)
            {
                _logger.LogError(ex, "Tag not found");
                return NotFound(new { Message = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving event tags");
                return StatusCode(500, new { Message = "An error occurred while retrieving event tags" });
            }
        }

        [HttpDelete("{eventId}/{tagId}")]
        public async Task<IActionResult> DeleteEventTag(int eventId, int tagId)
        {
            try
            {
                _logger.LogInformation("Deleting event tag: EventId={EventId}, TagId={TagId}", eventId, tagId);
                bool success = await _eventTagService.DeleteEventTagAsync(eventId, tagId);
                return Ok(new { Message = "Event tag deleted successfully" });
            }
            catch (ArgumentException ex)
            {
                _logger.LogError(ex, "Invalid event ID or tag ID provided");
                return BadRequest(new { Message = ex.Message });
            }
            catch (KeyNotFoundException ex)
            {
                _logger.LogError(ex, "Event tag not found");
                return NotFound(new { Message = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting event tag");
                return StatusCode(500, new { Message = "An error occurred while deleting the event tag" });
            }
        }
    }
}
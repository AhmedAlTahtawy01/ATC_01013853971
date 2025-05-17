using DataAccess.Repositories;
using DataAccess.Models;
using Microsoft.Extensions.Logging;
using BusinessLogic.Helpers;

namespace BusinessLogic.Services
{
    public class EventTagService
    {
        private readonly EventTagRepo _eventTagRepo;
        private readonly ILogger<EventTagService> _logger;
        private readonly Shared _shared;

        public EventTagService(EventTagRepo eventTagRepo, ILogger<EventTagService> logger, Shared shared)
        {
            _eventTagRepo = eventTagRepo;
            _logger = logger;
            _shared = shared;
        }

        // Validate event tag before creating (Helper method)
        private void ValidateEventTag(EventTag eventTag)
        {
            if (eventTag == null)
            {
                _logger.LogError("Event tag data is null");
                throw new ArgumentNullException(nameof(eventTag));
            }

            if (eventTag.EventId <= 0)
            {
                _logger.LogError("Invalid event ID");
                throw new ArgumentException("Event ID must be greater than 0");
            }

            if (eventTag.TagId <= 0)
            {
                _logger.LogError("Invalid tag ID");
                throw new ArgumentException("Tag ID must be greater than 0");
            }
        }

        // Create event tag
        public async Task<bool> CreateEventTagAsync(EventTag eventTag)
        {
            ValidateEventTag(eventTag);

            // Check if event exists
            var eventExists = await _shared.IsEventExists(eventTag.EventId);
            if (!eventExists)
            {
                _logger.LogError("Event not found: {EventId}", eventTag.EventId);
                throw new KeyNotFoundException("Event not found");
            }

            // Check if tag exists
            var tagExists = await _shared.IsTagExists(eventTag.TagId);
            if (!tagExists)
            {
                _logger.LogError("Tag not found: {TagId}", eventTag.TagId);
                throw new KeyNotFoundException("Tag not found");
            }

            // Check if event tag already exists
            var exists = await _eventTagRepo.CheckEventTagExistsAsync(eventTag.EventId, eventTag.TagId);
            if (exists)
            {
                _logger.LogWarning("Event tag already exists: EventId={EventId}, TagId={TagId}", 
                    eventTag.EventId, eventTag.TagId);
                throw new InvalidOperationException("Event tag already exists");
            }

            try
            {
                _logger.LogInformation("Creating event tag: EventId={EventId}, TagId={TagId}", 
                    eventTag.EventId, eventTag.TagId);
                return await _eventTagRepo.CreateEventTagAsync(eventTag);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating event tag");
                throw;
            }
        }

        // Get all event tags
        public async Task<List<EventTag>> GetAllEventTagsAsync()
        {
            try
            {
                _logger.LogInformation("Retrieving all event tags");
                return await _eventTagRepo.GetAllEventTagsAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving all event tags");
                throw;
            }
        }

        // Get event tags by event ID
        public async Task<List<EventTag>> GetEventTagsByEventIdAsync(int eventId)
        {
            if (eventId <= 0)
            {
                _logger.LogError("Invalid event ID");
                throw new ArgumentException("Event ID must be greater than 0");
            }

            // Check if event exists
            var eventExists = await _shared.IsEventExists(eventId);
            if (!eventExists)
            {
                _logger.LogError("Event not found: {EventId}", eventId);
                throw new KeyNotFoundException("Event not found");
            }

            try
            {
                _logger.LogInformation("Retrieving event tags by EventId: {EventId}", eventId);
                return await _eventTagRepo.GetEventTagsByEventIdAsync(eventId);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving event tags by EventId: {EventId}", eventId);
                throw;
            }
        }

        // Get event tags by tag ID
        public async Task<List<EventTag>> GetEventTagsByTagIdAsync(int tagId)
        {
            if (tagId <= 0)
            {
                _logger.LogError("Invalid tag ID");
                throw new ArgumentException("Tag ID must be greater than 0");
            }

            // Check if tag exists
            var tagExists = await _shared.IsTagExists(tagId);
            if (!tagExists)
            {
                _logger.LogError("Tag not found: {TagId}", tagId);
                throw new KeyNotFoundException("Tag not found");
            }

            try
            {
                _logger.LogInformation("Retrieving event tags by TagId: {TagId}", tagId);
                return await _eventTagRepo.GetEventTagsByTagIdAsync(tagId);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving event tags by TagId: {TagId}", tagId);
                throw;
            }
        }

        // Delete event tag
        public async Task<bool> DeleteEventTagAsync(int eventId, int tagId)
        {
            if (eventId <= 0)
            {
                _logger.LogError("Invalid event ID");
                throw new ArgumentException("Event ID must be greater than 0");
            }

            if (tagId <= 0)
            {
                _logger.LogError("Invalid tag ID");
                throw new ArgumentException("Tag ID must be greater than 0");
            }

            // Check if event tag exists
            var exists = await _eventTagRepo.CheckEventTagExistsAsync(eventId, tagId);
            if (!exists)
            {
                _logger.LogError("Event tag not found: EventId={EventId}, TagId={TagId}", eventId, tagId);
                throw new KeyNotFoundException("Event tag not found");
            }

            try
            {
                _logger.LogInformation("Deleting event tag: EventId={EventId}, TagId={TagId}", eventId, tagId);
                return await _eventTagRepo.DeleteEventTagAsync(eventId, tagId);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting event tag");
                throw;
            }
        }
    }
}
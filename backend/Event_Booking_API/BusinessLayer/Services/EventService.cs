using DataAccess.Repositories;
using DataAccess.Models;
using Microsoft.Extensions.Logging;
using BusinessLogic.Helpers;

namespace BusinessLogic.Services
{
    public class EventService
    {
        private readonly EventRepo _eventRepo;
        private readonly ILogger<EventService> _logger;
        private readonly Shared _shared;

        public EventService(EventRepo eventRepo, ILogger<EventService> logger, Shared shared)
        {
            _eventRepo = eventRepo;
            _logger = logger;
            _shared = shared;
        }

        // Validate event before creating or updating (Helper method)
        private void ValidateEvent(Event eventData, bool allowPastDate = false)
        {
            if (eventData == null)
            {
                _logger.LogError("Event data is null");
                throw new ArgumentNullException(nameof(eventData));
            }

            if (string.IsNullOrEmpty(eventData.Name))
            {
                _logger.LogError("Event name is null or empty");
                throw new ArgumentException("Event name cannot be null or empty");
            }

            if (!allowPastDate && eventData.Date < DateTime.UtcNow)
            {
                _logger.LogError("Event date is in the past");
                throw new ArgumentException("Event date cannot be in the past");
            }

            if (eventData.Price <= 0)
            {
                _logger.LogError("Event price is less than or equal to 0");
                throw new ArgumentException("Event price cannot be less than or equal to 0");
            }

            if (eventData.CreatedBy <= 0)
            {
                _logger.LogError("Event created by is less than or equal to 0");
                throw new ArgumentException("Event created by cannot be less than or equal to 0");
            }
        }

        // Validate string (Helper method)
        private void ValidateString(string value, string fieldName)
        {
            if (string.IsNullOrEmpty(value))
            {
                _logger.LogError($"{fieldName} is null or empty");
                throw new ArgumentException($"{fieldName} cannot be null or empty");
            }
        }

        // Create event
        public async Task<int> CreateEventAsync(Event eventData)
        {
            ValidateEvent(eventData);

            if (!await _shared.IsUserExists(eventData.CreatedBy))
            {
                _logger.LogError("User does not exist");
                throw new KeyNotFoundException("User does not exist");
            }

            try
            {
                _logger.LogInformation("Creating event: {Event}", eventData);
                int newEventId = await _eventRepo.CreateEventAsync(eventData);

                if (newEventId > 0)
                {
                    _logger.LogInformation("Event created successfully: {Event}", eventData);
                    return newEventId;
                }

                _logger.LogWarning("Failed to create event: {Event}", eventData);
                return 0;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating event");
                throw;
            }
        }

        // Get total count of events
        public async Task<int> GetEventCountAsync()
        {
            try
            {
                _logger.LogInformation("Retrieving total event count");
                return await _eventRepo.CountEventsAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while retrieving event count");
                throw;
            }
        }

        // Get all events with pagination
        public async Task<List<Event>> GetAllEventsAsync(int pageNumber = 1, int pageSize = 10)
        {
            if (pageNumber <= 0 || pageSize <= 0)
            {
                _logger.LogError("Invalid page number or page size");
                throw new ArgumentException("Page number and page size must be greater than 0");
            }

            try
            {
                _logger.LogInformation("Retrieving all events with pagination");
                return await _eventRepo.GetAllEventsAsync(pageNumber, pageSize);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving all events with pagination");
                throw;
            }
        }

        // Get event by ID
        public async Task<Event?> GetEventByIdAsync(int eventId)
        {
            if (eventId <= 0)
            {
                _logger.LogError("Invalid event ID");
                throw new ArgumentException("Event ID must be greater than 0");
            }

            try
            {
                _logger.LogInformation("Retrieving event by ID: {EventId}", eventId);
                var eventData = await _eventRepo.GetEventByIdAsync(eventId);

                if (eventData == null)
                {
                    _logger.LogError("Event not found with ID: {EventId}", eventId);
                    throw new KeyNotFoundException("Event not found");
                }

                return eventData;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving event by ID: {EventId}", eventId);
                throw;
            }
        }

        // Get events by isActive
        public async Task<List<Event>> GetEventsByIsActiveAsync(bool isActive)
        {
            try
            {
                _logger.LogInformation("Retrieving events by isActive: {IsActive}", isActive);
                return await _eventRepo.GetEventsByIsActiveAsync(isActive);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving events by isActive: {IsActive}", isActive);
                throw;
            }
        }

        // Get events by CreatedBy
        public async Task<List<Event>> GetEventsByCreatedByAsync(int createdBy)
        {
            if (createdBy <= 0)
            {
                _logger.LogError("Invalid createdBy value");
                throw new ArgumentException("CreatedBy must be greater than 0");
            }

            if (!await _shared.IsUserExists(createdBy))
            {
                _logger.LogError("User does not exist");
                throw new KeyNotFoundException("User does not exist");
            }

            try
            {
                _logger.LogInformation("Retrieving events by CreatedBy: {CreatedBy}", createdBy);
                return await _eventRepo.GetEventsByCreatedByAsync(createdBy);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving events by CreatedBy: {CreatedBy}", createdBy);
                throw;
            }
        }

        // Get events by name
        public async Task<List<Event>> GetEventsByNameAsync(string name)
        {
            ValidateString(name, "Name");

            try
            {
                _logger.LogInformation("Retrieving events by name: {Name}", name);
                return await _eventRepo.GetEventsByNameAsync(name);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving events by name: {Name}", name);
                throw;
            }
        }

        // Get events by description
        public async Task<List<Event>> GetEventsByDescriptionAsync(string description)
        {
            ValidateString(description, "Description");

            try
            {
                _logger.LogInformation("Retrieving events by description: {Description}", description);
                return await _eventRepo.GetEventsByDescriptionAsync(description);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving events by description: {Description}", description);
                throw;
            }
        }

        // Get events by category
        public async Task<List<Event>> GetEventsByCategoryAsync(string category)
        {
            ValidateString(category, "Category");

            try
            {
                _logger.LogInformation("Retrieving events by category: {Category}", category);
                return await _eventRepo.GetEventsByCategoryAsync(category);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving events by category: {Category}", category);
                throw;
            }
        }

        // Get events by venue
        public async Task<List<Event>> GetEventsByVenueAsync(string venue)
        {
            ValidateString(venue, "Venue");

            try
            {
                _logger.LogInformation("Retrieving events by venue: {Venue}", venue);
                return await _eventRepo.GetEventsByVenueAsync(venue);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving events by venue: {Venue}", venue);
                throw;
            }
        }

        // Get events by date
        public async Task<List<Event>> GetEventsByDateAsync(DateTime date)
        {
            try
            {
                _logger.LogInformation("Retrieving events by date: {Date}", date);
                return await _eventRepo.GetEventsByDateAsync(date);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving events by date: {Date}", date);
                throw;
            }
        }

        // Get events by price
        public async Task<List<Event>> GetEventsByPriceAsync(decimal price)
        {
            if (price <= 0)
            {
                _logger.LogError("Invalid price");
                throw new ArgumentException("Price cannot be less than or equal to 0");
            }

            try
            {
                _logger.LogInformation("Retrieving events by price: {Price}", price);
                return await _eventRepo.GetEventsByPriceAsync(price);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving events by price: {Price}", price);
                throw;
            }
        }

        // Update event
        public async Task<bool> UpdateEventAsync(Event eventData)
        {
            if (eventData.Id <= 0)
            {
                _logger.LogError("Invalid event ID: {EventId}", eventData.Id);
                throw new ArgumentException("Event ID must be greater than 0");
            }

            ValidateEvent(eventData);

            // Check if event exists
            var existingEvent = await GetEventByIdAsync(eventData.Id);
            if (existingEvent == null)
            {
                _logger.LogError("Event not found: {EventId}", eventData.Id);
                throw new InvalidOperationException("Event not found");
            }

            try
            {
                _logger.LogInformation("Updating event: {Event}", eventData);
                return await _eventRepo.UpdateEventAsync(eventData);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating event");
                throw;
            }
        }

        // Delete event
        public async Task<bool> DeleteEventAsync(int eventId)
        {
            if (eventId <= 0)
            {
                _logger.LogError("Invalid event ID: {EventId}", eventId);
                throw new ArgumentException("Event ID must be greater than 0");
            }

            try
            {
                _logger.LogInformation("Deleting event by ID: {EventId}", eventId);
                return await _eventRepo.DeleteEventAsync(eventId);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting event by ID: {EventId}", eventId);
                throw;
            }
        }
    }
}

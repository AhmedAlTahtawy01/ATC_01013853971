using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using DataAccess.Repositories;

namespace BusinessLogic.Helpers
{
    public class Shared
    {
        private readonly ILogger<Shared> _logger;
        private readonly UserRepo _userRepo;
        private readonly EventRepo _eventRepo;
        private readonly TagRepo _tagRepo;


        public Shared(ILogger<Shared> logger, UserRepo userRepo, EventRepo eventRepo, TagRepo tagRepo)
        {
            _logger = logger;
            _userRepo = userRepo;
            _eventRepo = eventRepo;
            _tagRepo = tagRepo;
        }

        public async Task<bool> IsUserExists(int userId)
        {
            _logger.LogInformation("Checking if user exists with ID: {UserId}", userId);
            var existingUser = await _userRepo.GetByIdAsync(userId);
            if (existingUser == null)
            {
                _logger.LogWarning("User not found with ID: {UserId}", userId);
                return false;
            }
            return true;
        }

        public async Task<bool> IsEventExists(int eventId)
        {
            _logger.LogInformation("Checking if event exists with ID: {EventId}", eventId);
            var existingEvent = await _eventRepo.GetEventByIdAsync(eventId);
            if (existingEvent == null)
            {
                _logger.LogWarning("Event not found with ID: {EventId}", eventId);
                return false;
            }
            return true;
        }

        public async Task<bool> IsTagExists(int tagId)
        {
            _logger.LogInformation("Checking if tag exists with ID: {TagId}", tagId);
            var existingTag = await _tagRepo.GetTagByIdAsync(tagId);
            if (existingTag == null)
            {
                _logger.LogWarning("Tag not found with ID: {TagId}", tagId);
                return false;
            }
            return true;
        }
    }
}

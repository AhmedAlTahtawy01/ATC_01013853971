using DataAccess.Repositories;
using DataAccess.Models;
using Microsoft.Extensions.Logging;

namespace BusinessLogic.Services
{
    public class TagService
    {
        private readonly TagRepo _tagRepo;
        private readonly ILogger<TagService> _logger;

        public TagService(TagRepo tagRepo, ILogger<TagService> logger)
        {
            _tagRepo = tagRepo;
            _logger = logger;
        }

        // Validate tag before creating or updating (Helper method)
        private void ValidateTag(Tag tag)
        {
            if (tag == null)
            {
                _logger.LogError("Tag data is null");
                throw new ArgumentNullException(nameof(tag));
            }

            if (string.IsNullOrEmpty(tag.Name))
            {
                _logger.LogError("Tag name is null or empty");
                throw new ArgumentException("Tag name cannot be null or empty");
            }

            if (tag.Name.Length > 30)
            {
                _logger.LogError("Tag name exceeds maximum length of 30 characters");
                throw new ArgumentException("Tag name cannot exceed 30 characters");
            }
        }

        // Create tag
        public async Task<int> CreateTagAsync(Tag tag)
        {
            ValidateTag(tag);

            // Check if tag with same name exists
            var existingTag = await _tagRepo.GetTagByNameAsync(tag.Name);
            if (existingTag != null)
            {
                _logger.LogWarning("Tag with name '{TagName}' already exists", tag.Name);
                throw new InvalidOperationException("Tag with this name already exists");
            }

            try
            {
                _logger.LogInformation("Creating tag: {Tag}", tag);
                int newTagId = await _tagRepo.CreateTagAsync(tag);

                if (newTagId > 0)
                {
                    _logger.LogInformation("Tag created successfully: {Tag}", tag);
                    return newTagId;
                }

                _logger.LogWarning("Failed to create tag: {Tag}", tag);
                return 0;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating tag");
                throw;
            }
        }

        // Get all tags with pagination
        public async Task<List<Tag>> GetAllTagsAsync(int pageNumber = 1, int pageSize = 10)
        {
            if (pageNumber <= 0 || pageSize <= 0)
            {
                _logger.LogError("Invalid page number or page size");
                throw new ArgumentException("Page number and page size must be greater than 0");
            }

            try
            {
                _logger.LogInformation("Retrieving all tags with pagination");
                return await _tagRepo.GetAllTagsAsync(pageNumber, pageSize);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving all tags with pagination");
                throw;
            }
        }

        // Get tag by ID
        public async Task<Tag?> GetTagByIdAsync(int tagId)
        {
            if (tagId <= 0)
            {
                _logger.LogError("Invalid tag ID");
                throw new ArgumentException("Tag ID must be greater than 0");
            }

            try
            {
                _logger.LogInformation("Retrieving tag by ID: {TagId}", tagId);
                var tag = await _tagRepo.GetTagByIdAsync(tagId);

                if (tag == null)
                {
                    _logger.LogError("Tag not found with ID: {TagId}", tagId);
                    throw new KeyNotFoundException("Tag not found");
                }

                return tag;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving tag by ID: {TagId}", tagId);
                throw;
            }
        }

        // Get tag by name
        public async Task<Tag?> GetTagByNameAsync(string name)
        {
            if (string.IsNullOrEmpty(name))
            {
                _logger.LogError("Tag name is null or empty");
                throw new ArgumentException("Tag name cannot be null or empty");
            }

            try
            {
                _logger.LogInformation("Retrieving tag by name: {Name}", name);
                var tag = await _tagRepo.GetTagByNameAsync(name);

                if (tag == null)
                {
                    _logger.LogError("Tag not found with name: {Name}", name);
                    throw new KeyNotFoundException("Tag not found");
                }

                return tag;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving tag by name: {Name}", name);
                throw;
            }
        }

        // Update tag
        public async Task<bool> UpdateTagAsync(Tag tag)
        {
            if (tag.Id <= 0)
            {
                _logger.LogError("Invalid tag ID: {TagId}", tag.Id);
                throw new ArgumentException("Tag ID must be greater than 0");
            }

            ValidateTag(tag);

            // Check if tag exists
            var existingTag = await GetTagByIdAsync(tag.Id);
            if (existingTag == null)
            {
                _logger.LogError("Tag not found: {TagId}", tag.Id);
                throw new KeyNotFoundException("Tag not found");
            }

            // Check if another tag with the same name exists
            var tagWithSameName = await _tagRepo.GetTagByNameAsync(tag.Name);
            if (tagWithSameName != null && tagWithSameName.Id != tag.Id)
            {
                _logger.LogWarning("Another tag with name '{TagName}' already exists", tag.Name);
                throw new InvalidOperationException("Another tag with this name already exists");
            }

            try
            {
                _logger.LogInformation("Updating tag: {Tag}", tag);
                return await _tagRepo.UpdateTagAsync(tag);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating tag");
                throw;
            }
        }

        // Delete tag
        public async Task<bool> DeleteTagAsync(int tagId)
        {
            if (tagId <= 0)
            {
                _logger.LogError("Invalid tag ID: {TagId}", tagId);
                throw new ArgumentException("Tag ID must be greater than 0");
            }

            try
            {
                _logger.LogInformation("Deleting tag by ID: {TagId}", tagId);
                return await _tagRepo.DeleteTagAsync(tagId);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting tag by ID: {TagId}", tagId);
                throw;
            }
        }
    }
}
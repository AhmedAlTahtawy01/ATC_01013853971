using Microsoft.AspNetCore.Mvc;
using BusinessLogic.Services;
using DataAccess.Models;
using Microsoft.AspNetCore.Authorization;

namespace EventBooking.Controllers
{
    [Authorize]
    [ApiController]
    [Route("api/[controller]")]
    public class TagController : ControllerBase
    {
        private readonly TagService _tagService;
        private readonly ILogger<TagController> _logger;

        public TagController(TagService tagService, ILogger<TagController> logger)
        {
            _tagService = tagService;
            _logger = logger;
        }

        [HttpPost]
        public async Task<IActionResult> CreateTag([FromBody] Tag tag)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    _logger.LogError("Invalid tag data");
                    return BadRequest(new { message = "Invalid tag data", errors = ModelState });
                }

                if (string.IsNullOrWhiteSpace(tag.Name))
                {
                    _logger.LogError("Tag name is required");
                    return BadRequest(new { message = "Tag name is required" });
                }

                _logger.LogInformation("Creating new tag");
                int tagId = await _tagService.CreateTagAsync(tag);
                return Ok(new { message = "Tag created successfully", tagId });
            }
            catch (ArgumentNullException ex)
            {
                _logger.LogError(ex, "Invalid tag data provided");
                return BadRequest(new { message = ex.Message });
            }
            catch (ArgumentException ex)
            {
                _logger.LogError(ex, "Invalid tag data provided");
                return BadRequest(new { message = ex.Message });
            }
            catch (InvalidOperationException ex)
            {
                _logger.LogError(ex, "Tag creation failed due to business rule violation");
                return Conflict(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating tag");
                return StatusCode(500, new { message = "An error occurred while creating the tag" });
            }
        }

        [HttpGet]
        public async Task<IActionResult> GetAllTags([FromQuery] int pageNumber = 1, [FromQuery] int pageSize = 10)
        {
            try
            {
                _logger.LogInformation("Retrieving all tags with pagination");
                var tags = await _tagService.GetAllTagsAsync(pageNumber, pageSize);

                if (tags.Count == 0)
                {
                    _logger.LogWarning("No tags found");
                    return NotFound(new { message = "No tags found" });
                }

                return Ok(new { message = "Tags retrieved successfully", tags });
            }
            catch (ArgumentException ex)
            {
                _logger.LogError(ex, "Invalid pagination parameters");
                return BadRequest(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving tags");
                return StatusCode(500, new { message = "An error occurred while retrieving tags" });
            }
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetTagById(int id)
        {
            try
            {
                _logger.LogInformation("Retrieving tag by ID: {TagId}", id);
                var tag = await _tagService.GetTagByIdAsync(id);

                if (tag == null)
                {
                    _logger.LogWarning("Tag not found with ID: {TagId}", id);
                    return NotFound(new { message = "Tag not found" });
                }

                return Ok(new { message = "Tag retrieved successfully", tag });
            }
            catch (ArgumentException ex)
            {
                _logger.LogError(ex, "Invalid tag ID provided");
                return BadRequest(new { message = ex.Message });
            }
            catch (KeyNotFoundException ex)
            {
                _logger.LogError(ex, "Tag not found");
                return NotFound(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving tag");
                return StatusCode(500, new { message = "An error occurred while retrieving the tag" });
            }
        }

        [HttpGet("name/{name}")]
        public async Task<IActionResult> GetTagByName(string name)
        {
            try
            {
                _logger.LogInformation("Retrieving tag by name: {Name}", name);
                var tag = await _tagService.GetTagByNameAsync(name);

                if (tag == null)
                {
                    _logger.LogWarning("Tag not found with name: {Name}", name);
                    return NotFound(new { message = "Tag not found" });
                }

                return Ok(new { message = "Tag retrieved successfully", tag });
            }
            catch (ArgumentException ex)
            {
                _logger.LogError(ex, "Invalid tag name provided");
                return BadRequest(new { message = ex.Message });
            }
            catch (KeyNotFoundException ex)
            {
                _logger.LogError(ex, "Tag not found");
                return NotFound(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving tag");
                return StatusCode(500, new { message = "An error occurred while retrieving the tag" });
            }
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateTag(int id, [FromBody] Tag tag)
        {
            if (id != tag.Id)
            {
                _logger.LogError("Tag ID mismatch: {UrlId} vs {BodyId}", id, tag.Id);
                return BadRequest(new { Message = "Tag ID in URL does not match the ID in the request body" });
            }

            try
            {
                _logger.LogInformation("Updating tag: {Tag}", tag);
                bool success = await _tagService.UpdateTagAsync(tag);
                return Ok(new { Message = "Tag updated successfully" });
            }
            catch (ArgumentNullException ex)
            {
                _logger.LogError(ex, "Invalid tag data provided");
                return BadRequest(new { Message = ex.Message });
            }
            catch (ArgumentException ex)
            {
                _logger.LogError(ex, "Invalid tag data provided");
                return BadRequest(new { Message = ex.Message });
            }
            catch (KeyNotFoundException ex)
            {
                _logger.LogError(ex, "Tag not found");
                return NotFound(new { Message = ex.Message });
            }
            catch (InvalidOperationException ex)
            {
                _logger.LogError(ex, "Tag update failed due to business rule violation");
                return Conflict(new { Message = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating tag");
                return StatusCode(500, new { Message = "An error occurred while updating the tag" });
            }
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteTag(int id)
        {
            try
            {
                _logger.LogInformation("Deleting tag by ID: {TagId}", id);
                bool success = await _tagService.DeleteTagAsync(id);
                return Ok(new { Message = "Tag deleted successfully" });
            }
            catch (ArgumentException ex)
            {
                _logger.LogError(ex, "Invalid tag ID provided");
                return BadRequest(new { Message = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting tag");
                return StatusCode(500, new { Message = "An error occurred while deleting the tag" });
            }
        }
    }
}
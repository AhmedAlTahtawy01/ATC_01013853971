using System.ComponentModel.DataAnnotations;

namespace DataAccess.Models
{
    public class EventTag
    {
        [Required]
        public required int EventId { get; set; }
        [Required]
        public required int TagId { get; set; }
    }
}

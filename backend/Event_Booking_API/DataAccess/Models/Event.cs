using System.ComponentModel.DataAnnotations;

namespace DataAccess.Models
{
    public class Event
    {
        public int Id { get; set; }

        [Required]
        [StringLength(100)]
        public required string Name { get; set; }

        public string? Description { get; set; }

        [StringLength(50)]
        public string? Category { get; set; }

        [StringLength(100)]
        public string? Venue { get; set; }

        [Required]
        public DateTime Date { get; set; }

        [Range(0, 100000)]
        public decimal Price { get; set; }

        public string? ImageUrl { get; set; }

        [Required]
        public bool IsActive { get; set; }

        [Required]
        public int CreatedBy { get; set; }
    }
}

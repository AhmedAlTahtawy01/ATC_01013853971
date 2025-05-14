using System.ComponentModel.DataAnnotations;

namespace ATC.DataAccess.Models
{
    public class Booking
    {
        public int Id { get; set; }

        [Required]
        public int UserId { get; set; }

        [Required]
        public int EventId { get; set; }

        public DateTime BookedAt { get; set; } = DateTime.Now;
    }
}

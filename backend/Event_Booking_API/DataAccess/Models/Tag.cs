using System.ComponentModel.DataAnnotations;

namespace DataAccess.Models
{
    public class Tag
    {
        public int Id { get; set; }

        [Required]
        [StringLength(30)]
        public required string Name { get; set; }
    }
}

using System.ComponentModel.DataAnnotations;

namespace ATC.DataAccess.Models
{
    public class Role
    {
        public int Id { get; set; }

        [Required]
        [StringLength(20)]
        public required string Name { get; set; }
    }
}

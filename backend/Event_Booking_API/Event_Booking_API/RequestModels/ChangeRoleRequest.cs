using System.ComponentModel.DataAnnotations;

namespace EventBooking.RequestModels
{
    public class ChangeRoleRequest
    {
        [Required]
        public required int UserId { get; set; }

        [Required]
        public required int NewRoleId { get; set; }
    }
}
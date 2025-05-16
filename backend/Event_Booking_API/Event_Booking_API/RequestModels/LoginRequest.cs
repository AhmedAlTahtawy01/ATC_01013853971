using System.ComponentModel.DataAnnotations;

namespace EventBooking.RequestModels
{
    public class LoginRequest
    {
        [Required]
        public required string Username { get; set; }

        [Required]
        [MinLength(8)]
        public required string Password { get; set; }
    }
}

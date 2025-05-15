namespace ATC.EventBooking.RequestModels;
using System.ComponentModel.DataAnnotations;

public class ChangeRoleRequest
{
    [Required]
    public required int UserId { get; set; }

    [Required]
    public required int NewRoleId { get; set; }
} 
﻿using System.ComponentModel.DataAnnotations;

namespace DataAccess.Models
{
    public class User
    {
        public int Id { get; set; }

        [Required]
        [StringLength(50)]
        public required string Username { get; set; }

        [Required]
        [StringLength(100)]
        public required string Name { get; set; }

        [Required]
        [EmailAddress]
        public required string Email { get; set; }

        [Required]
        [MinLength(8)]
        public required string PasswordHash { get; set; }

        [Required]
        public int RoleId { get; set; }
    }
}

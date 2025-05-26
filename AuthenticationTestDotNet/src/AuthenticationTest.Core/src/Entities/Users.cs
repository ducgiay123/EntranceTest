using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using AuthenticationTest.Core.src.Entities;
using AuthenticationTest.Core.src.Utilities;

namespace AuthenticationTest.src.Core.Entities
{
    public class Users
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "First name is required.")]
        [MinLength(2, ErrorMessage = "First name must be at least 2 characters.")]
        [MaxLength(32, ErrorMessage = "First name cannot exceed 32 characters.")]
        [RegularExpression(@"^[a-zA-Z\s]*$", ErrorMessage = "First name must contain only letters and spaces.")]
        public string FirstName { get; set; } = string.Empty;

        [Required(ErrorMessage = "Last name is required.")]
        [MinLength(2, ErrorMessage = "Last name must be at least 2 characters.")]
        [MaxLength(32, ErrorMessage = "Last name cannot exceed 32 characters.")]
        [RegularExpression(@"^[a-zA-Z\s]*$", ErrorMessage = "Last name must contain only letters and spaces.")]
        public string LastName { get; set; } = string.Empty;

        [Required(ErrorMessage = "Email is required.")]
        [EmailAddress(ErrorMessage = "Invalid email format.")]
        [MaxLength(64, ErrorMessage = "Email cannot exceed 64 characters.")]
        public string Email { get; set; } = string.Empty;

        [Required(ErrorMessage = "Password hash is required.")]
        [MaxLength(255, ErrorMessage = "Password hash cannot exceed 255 characters.")]
        public string Hash { get; set; } = string.Empty;

        [Required]
        public DateTime CreatedAt { get; set; }

        [Required]
        public DateTime UpdatedAt { get; set; }

        [Required]
        public UserRole Role { get; set; } = UserRole.User;

        public ICollection<Tokens> Tokens { get; set; } = new List<Tokens>();
        public ICollection<Tasks> Tasks { get; set; } = new List<Tasks>();
    }

}

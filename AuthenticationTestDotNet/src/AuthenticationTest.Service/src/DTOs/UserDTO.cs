using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;
using AuthenticationTest.Core.src.Utilities;

namespace AuthenticationTest.Service.src.DTOs
{
    public class UserDTO
    {
        public class UpdateUserDto
        {
            [Required]
            [EmailAddress]
            public string Email { get; set; } = string.Empty;

            [MinLength(2), MaxLength(32)]
            public string FirstName { get; set; } = string.Empty;

            [MinLength(2), MaxLength(32)]
            public string LastName { get; set; } = string.Empty;
        }
        public class UserReadDto
        {
            public int Id { get; set; }
            public string FirstName { get; set; } = string.Empty;
            public string LastName { get; set; } = string.Empty;
            public string Email { get; set; } = string.Empty;
            public DateTime CreatedAt { get; set; }
            public DateTime UpdatedAt { get; set; }
            public UserRole Role { get; set; }
        }
    }
}
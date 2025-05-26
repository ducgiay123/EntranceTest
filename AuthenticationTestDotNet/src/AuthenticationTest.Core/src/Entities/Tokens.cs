using System;
using System.ComponentModel.DataAnnotations;

namespace AuthenticationTest.src.Core.Entities
{
    public class Tokens
    {
        public int Id { get; set; }

        [Required]
        public int UserId { get; set; }

        [Required(ErrorMessage = "Refresh token is required.")]
        [MaxLength(250, ErrorMessage = "Refresh token cannot exceed 250 characters.")]
        public string RefreshToken { get; set; } = string.Empty;

        [Required(ErrorMessage = "ExpiresIn is required.")]
        [MaxLength(64, ErrorMessage = "ExpiresIn cannot exceed 64 characters.")]
        public string ExpiresIn { get; set; } = string.Empty;

        [Required]
        public DateTime CreatedAt { get; set; }

        [Required]
        public DateTime UpdatedAt { get; set; }

        public Users User { get; set; } = null!;
    }
}

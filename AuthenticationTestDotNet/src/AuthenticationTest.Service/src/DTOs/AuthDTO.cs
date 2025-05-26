using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AuthenticationTest.Core.src.Utilities;

namespace AuthenticationTest.Service.src.DTOs
{
    public class SignUpRequestDto
    {
        [Required(ErrorMessage = "Email is required.")]
        [StringLength(64, ErrorMessage = "Email cannot exceed 64 characters.")]
        [EmailAddress(ErrorMessage = "Invalid email format.")]
        public string Email { get; set; } = string.Empty;

        [Required(ErrorMessage = "Password is required.")]
        [StringLength(20, MinimumLength = 8, ErrorMessage = "Password must be between 8 and 20 characters.")]
        public string Password { get; set; } = string.Empty;

        [Required(ErrorMessage = "First name is required.")]
        [StringLength(32, ErrorMessage = "First name cannot exceed 32 characters.")]
        public string FirstName { get; set; } = string.Empty;

        [Required(ErrorMessage = "Last name is required.")]
        [StringLength(32, ErrorMessage = "Last name cannot exceed 32 characters.")]
        public string LastName { get; set; } = string.Empty;
    }

    public class SignUpResponseDto
    {
        public int Id { get; set; }
        public string FirstName { get; set; } = string.Empty;
        public string LastName { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
    }

    public class LoginRequestDto
    {
        [Required(ErrorMessage = "Email is required.")]
        [StringLength(64, ErrorMessage = "Email cannot exceed 64 characters.")]
        [EmailAddress(ErrorMessage = "Invalid email format.")]
        public string Email { get; set; } = string.Empty;

        [Required(ErrorMessage = "Password is required.")]
        [StringLength(20, MinimumLength = 8, ErrorMessage = "Password must be between 8 and 20 characters.")]
        public string Password { get; set; } = string.Empty;
    }

    public class LoginResponseDto
    {
        public UserDto User { get; set; } = null!;
        public string Token { get; set; } = string.Empty;
        public string RefreshToken { get; set; } = string.Empty;
    }

    public class UserDto
    {
        public string FirstName { get; set; } = string.Empty;
        public string LastName { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string DisplayName { get; set; } = string.Empty;
        public UserRole Role { get; set; } = UserRole.User;
    }

    public class RefreshTokenRequestDto
    {
        [Required(ErrorMessage = "Refresh token is required.")]
        public string RefreshToken { get; set; } = string.Empty;
    }

    public class RefreshTokenResponseDto
    {
        public string Token { get; set; } = string.Empty;
        public string RefreshToken { get; set; } = string.Empty;
    }
}

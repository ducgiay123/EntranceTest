using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AuthenticationTest.Service.src.DTOs
{

    public class SignUpRequestDto
    {
        [Required]
        [StringLength(64)]
        [EmailAddress]
        public string Email { get; set; }

        [Required]
        [StringLength(20, MinimumLength = 8)]
        public string Password { get; set; }

        [Required]
        [StringLength(32)]
        public string FirstName { get; set; }

        [Required]
        [StringLength(32)]
        public string LastName { get; set; }
    }
    public class SignUpResponseDto
    {
        public int Id { get; set; }

        public string FirstName { get; set; }

        public string LastName { get; set; }

        public string Email { get; set; }

        public string DisplayName => $"{FirstName} {LastName}";
    }
    public class LoginRequestDto
    {
        [Required]
        [StringLength(64)]
        [EmailAddress]
        public string Email { get; set; }

        [Required]
        [StringLength(20, MinimumLength = 8)]
        public string Password { get; set; }
    }

    public class LoginResponseDto
    {
        public UserDto User { get; set; }
        public string Token { get; set; }
        public string RefreshToken { get; set; }
    }
    public class UserDto
    {
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string Email { get; set; }
        public string DisplayName { get; set; }
    }
    public class LogoutRequestDto
    {
        public string RefreshToken { get; set; }
    }
    public class RefreshTokenRequestDto
    {
        public string RefreshToken { get; set; }
    }

    public class RefreshTokenResponseDto
    {
        public string Token { get; set; }
        public string RefreshToken { get; set; }
    }

    // Response DTO
    public class TokenResponseDto
    {
        public string Token { get; set; }
        public string RefreshToken { get; set; }
    }
}

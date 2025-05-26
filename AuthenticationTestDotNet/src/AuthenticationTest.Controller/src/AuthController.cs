using System;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Threading.Tasks;
using AuthenticationTest.Service.src.Abstracts;
using AuthenticationTest.Service.src.DTOs;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

namespace AuthenticationTest.Controller.src
{
    [ApiController]
    [Route("api/v1/auth")]
    public class AuthController : ControllerBase
    {
        private readonly IAuthService _authService;
        private readonly ILogger<AuthController> _logger;
        public AuthController(IAuthService authService, ILogger<AuthController> logger)
        {
            _authService = authService ?? throw new ArgumentNullException(nameof(authService));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }
        [Authorize]
        [HttpGet("user")]
        public async Task<IActionResult> GetUser()
        {
            try
            {
                // Log claims for debugging
                var claims = User.Claims.Select(c => $"{c.Type}: {c.Value}").ToList();
                Console.WriteLine("Claims: " + string.Join(", ", claims));

                // Get email from JWT
                var emailClaim = User.FindFirst(ClaimTypes.Email)?.Value
                    ?? User.FindFirst(JwtRegisteredClaimNames.Sub)?.Value;
                if (string.IsNullOrEmpty(emailClaim))
                {
                    Console.WriteLine("Invalid or missing email in authentication token.");
                    return Unauthorized("Invalid or missing email in authentication token.");
                }

                // Get user from service
                var user = await _authService.GetUserAsync(emailClaim);
                if (user == null)
                {
                    Console.WriteLine($"User with email {emailClaim} not found.");
                    return NotFound("User not found.");
                }

                // Return user data
                var userResponse = new
                {
                    id = user.Id,
                    email = user.Email,
                    firstName = user.FirstName,
                    lastName = user.LastName
                };

                return Ok(userResponse);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error: {ex.Message}");
                return StatusCode(500, "Internal server error.");
            }
        }
        [HttpPost("signup")]
        public async Task<IActionResult> SignUp([FromBody] SignUpRequestDto dto)
        {
            if (dto == null)
                return BadRequest("Invalid request body.");

            try
            {
                var user = await _authService.SignUpAsync(dto);

                var response = new SignUpResponseDto
                {
                    Id = user.Id,
                    FirstName = user.FirstName,
                    LastName = user.LastName,
                    Email = user.Email,
                };

                return CreatedAtAction(nameof(SignUp), new { email = user.Email }, response);
            }
            catch (ArgumentException ex)
            {
                return BadRequest(ex.Message);
            }
            catch (InvalidOperationException ex)
            {
                return Conflict(ex.Message);
            }
            catch (Exception)
            {
                return StatusCode(500, "Internal server error.");
            }
        }
        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] LoginRequestDto dto)
        {
            if (dto == null)
                return BadRequest("Invalid request body.");
            try
            {
                var response = await _authService.LoginAsync(dto);
                return Ok(response);
            }
            catch (ArgumentException ex)
            {
                return BadRequest(ex.Message);
            }
            catch (UnauthorizedAccessException ex)
            {
                return Unauthorized(ex.Message);
            }
            catch (Exception)
            {
                return StatusCode(500, "Internal server error.");
            }
        }

        [Authorize]
        [HttpPost("logout")]
        public async Task<IActionResult> Logout()
        {
            try
            {
                // Log all claims for debugging
                var claims = User.Claims.Select(c => $"{c.Type}: {c.Value}").ToList();
                Console.WriteLine("Claims: " + string.Join(", ", claims));

                // Get userId for logging/debugging
                var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value
                    ?? User.FindFirst("http://schemas.xmlsoap.org/ws/2005/05/identity/claims/nameidentifier")?.Value;
                if (string.IsNullOrEmpty(userIdClaim) || !int.TryParse(userIdClaim, out var userId) || userId <= 0)
                {
                    Console.WriteLine("Invalid or missing user ID in authentication token.");
                    return Unauthorized("Invalid or missing user ID in authentication token.");
                }

                // Get email from JWT
                var emailClaim = User.FindFirst(ClaimTypes.Email)?.Value
                    ?? User.FindFirst(JwtRegisteredClaimNames.Sub)?.Value;
                if (string.IsNullOrEmpty(emailClaim))
                {
                    Console.WriteLine("Invalid or missing email in authentication token.");
                    return Unauthorized("Invalid or missing email in authentication token.");
                }

                Console.WriteLine($"User ID: {userId}, Email: {emailClaim}");
                bool result = await _authService.LogoutAsync(emailClaim); // Pass email instead of userIdClaim
                if (!result)
                    return BadRequest("Invalid or expired refresh token.");

                return Ok("Logged out successfully.");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error: {ex.Message}");
                return StatusCode(500, "Internal server error.");
            }
        }
        [HttpPost("refreshToken")]
        public async Task<IActionResult> Refresh([FromBody] RefreshTokenRequestDto dto)
        {
            try
            {
                if (dto == null || string.IsNullOrWhiteSpace(dto.RefreshToken))
                    return BadRequest("Refresh token is required.");

                var response = await _authService.RefreshTokenAsync(dto.RefreshToken);
                return Ok(response);
            }
            catch (ArgumentException ex)
            {
                return BadRequest(ex.Message);
            }
            catch (KeyNotFoundException)
            {
                return NotFound("Invalid or expired refresh token.");
            }
            catch (Exception)
            {
                return StatusCode(500, "Internal server error.");
            }
        }
        [HttpGet("hello")]
        public IActionResult GetHello()
        {
            return Ok("hello");
        }
    }
}

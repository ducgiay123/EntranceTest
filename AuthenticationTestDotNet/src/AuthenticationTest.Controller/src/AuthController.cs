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

        [HttpPost("signup")]
        public async Task<IActionResult> SignUp([FromBody] SignUpRequestDto dto)
        {
            if (dto == null)
                return BadRequest(new { error = "Invalid request body." });

            try
            {
                var user = await _authService.SignUpAsync(dto);

                return CreatedAtAction(nameof(SignUp), new
                {
                    user.Email
                }, new
                {
                    user.FirstName,
                    user.LastName,
                    user.Email
                });
            }
            catch (ArgumentException ex)
            {
                return BadRequest(new { error = ex.Message });
            }
            catch (InvalidOperationException ex)
            {
                return Conflict(new { error = ex.Message });
            }
            catch (Exception)
            {
                return StatusCode(500, new { error = "Internal server error." });
            }
        }
        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] LoginRequestDto dto)
        {
            try
            {
                var response = await _authService.LoginAsync(dto);
                return Ok(response);
            }
            catch (ArgumentException ex)
            {
                return BadRequest(new { error = ex.Message });
            }
            catch (UnauthorizedAccessException ex)
            {
                return Unauthorized(new { error = ex.Message });
            }
            catch (Exception)
            {
                return StatusCode(500, new { error = "Internal server error." });
            }
        }
        [Authorize]
        [HttpPost("logout")]
        public async Task<IActionResult> Logout([FromBody] LogoutRequestDto dto)
        {
            try
            {
                var emailClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value
                    ?? User.FindFirst(JwtRegisteredClaimNames.Sub)?.Value;
                if (string.IsNullOrWhiteSpace(emailClaim))
                    return Unauthorized(new { error = "Invalid user authentication." });

                bool result = await _authService.LogoutAsync(emailClaim, dto.RefreshToken);
                if (!result)
                    return BadRequest(new { error = $"Invalid or expired refresh token.{emailClaim} " });

                return Ok(new { message = "Logged out successfully." });
            }
            catch (Exception)
            {
                return StatusCode(500, new { error = "Internal server error." });
            }
        }
        [HttpPost("refreshToken")]
        public async Task<IActionResult> Refresh([FromBody] RefreshTokenRequestDto dto)
        {
            try
            {
                if (dto == null || string.IsNullOrWhiteSpace(dto.RefreshToken))
                    return BadRequest(new { error = "Refresh token is required." });

                var response = await _authService.RefreshTokenAsync(dto.RefreshToken);
                return Ok(response);
            }
            catch (ArgumentException ex)
            {
                return BadRequest(new { error = ex.Message });
            }
            catch (KeyNotFoundException)
            {
                return NotFound(new { error = "Invalid or expired refresh token." });
            }
            catch (Exception)
            {
                return StatusCode(500, new { error = "Internal server error." });
            }
        }
        [HttpGet("hello")]
        public IActionResult GetHello()
        {
            return Ok("hello");
        }
    }
}

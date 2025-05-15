using AuthenticationTest.Core.RepoAbstract;
using AuthenticationTest.Service.src.Abstracts;
using AuthenticationTest.Service.src.DTOs;
using AuthenticationTest.src.Core.Entities;
using Konscious.Security.Cryptography;
using Microsoft.IdentityModel.Tokens;
using System;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using System.Security.Claims;
using Microsoft.Extensions.Configuration;
using System.Diagnostics;

namespace AuthenticationTest.Service.src.Services
{
    public class AuthService : IAuthService
    {
        private readonly IUserRepository _userRepository;
        private readonly string _jwtSecret;
        private readonly string _issuer;
        private readonly string _audience;

        public AuthService(IUserRepository userRepository, IConfiguration configuration)
        {
            _userRepository = userRepository ?? throw new ArgumentNullException(nameof(userRepository));
            _jwtSecret = configuration["Jwt:Secret"] ?? throw new ArgumentNullException("Jwt:Secret is missing in configuration.");
            _issuer = configuration["Jwt:Issuer"] ?? "AuthenticationTest";
            _audience = configuration["Jwt:Audience"] ?? "AuthenticationTest";
        }

        public async Task<SignUpResponseDto> SignUpAsync(SignUpRequestDto dto)
        {
            ValidateSignUpRequest(dto);

            if (!await _userRepository.IsEmailAvailableAsync(dto.Email))
                throw new InvalidOperationException("Email is already registered.");

            var user = new User
            {
                FirstName = dto.FirstName,
                LastName = dto.LastName,
                Email = dto.Email,
                Hash = await HashPasswordAsync(dto.Password),
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };
            var createdUser = await _userRepository.CreateAsync(user);

            return new SignUpResponseDto
            {
                FirstName = createdUser.FirstName,
                LastName = createdUser.LastName,
                Email = createdUser.Email
            };
        }

        public async Task<LoginResponseDto> LoginAsync(LoginRequestDto dto)
        {
            if (dto == null || string.IsNullOrWhiteSpace(dto.Email) || string.IsNullOrWhiteSpace(dto.Password))
                throw new ArgumentException("Email and password are required.");

            var user = await _userRepository.GetByEmailAsync(dto.Email);
            if (user == null || string.IsNullOrWhiteSpace(user.Hash))
                throw new UnauthorizedAccessException("Cannot find user with this email.");

            // Use the new VerifyPasswordAsync
            bool isPasswordValid = await VerifyPasswordAsync(dto.Password, user.Hash);
            if (!isPasswordValid)
                throw new UnauthorizedAccessException("Invalid email or password.");

            string jwtToken = GenerateJwtToken(user);
            string refreshToken = GenerateRefreshToken();

            await _userRepository.StoreRefreshTokenAsync(user.Id, refreshToken);

            return new LoginResponseDto
            {
                User = new UserDto
                {
                    FirstName = user.FirstName,
                    LastName = user.LastName,
                    Email = user.Email,
                    DisplayName = $"{user.FirstName} {user.LastName}"
                },
                Token = jwtToken,
                RefreshToken = refreshToken
            };
        }

        public async Task<bool> LogoutAsync(string email, string refreshToken)
        {
            if (string.IsNullOrWhiteSpace(email) || string.IsNullOrWhiteSpace(refreshToken))
                return false;

            return await _userRepository.RevokeRefreshTokenAsync(email, refreshToken);
        }

        public async Task<User?> GetUserByEmailAsync(string email)
        {
            return await _userRepository.GetByEmailAsync(email);
        }
        public async Task<RefreshTokenResponseDto> RefreshTokenAsync(string refreshToken)
        {
            if (string.IsNullOrWhiteSpace(refreshToken))
                throw new ArgumentException("Refresh token is required.");

            // Validate and get user associated with the refresh token
            var user = await _userRepository.ValidateRefreshTokenAsync(refreshToken);
            if (user == null)
                throw new KeyNotFoundException("Invalid or expired refresh token.");

            // Invalidate the old refresh token
            await _userRepository.InvalidateRefreshTokenAsync(user.Id, refreshToken);

            // Generate new tokens
            string newJwtToken = GenerateJwtToken(user);
            string newRefreshToken = GenerateRefreshToken();

            // Store the new refresh token
            await _userRepository.StoreRefreshTokenAsync(user.Id, newRefreshToken);

            return new RefreshTokenResponseDto
            {
                Token = newJwtToken,
                RefreshToken = newRefreshToken
            };
        }

        private string GenerateJwtToken(User user)
        {
            var claims = new[]
            {
                new Claim(ClaimTypes.NameIdentifier, user.Email),
                new Claim(JwtRegisteredClaimNames.Sub, user.Email),
                new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
                new Claim(ClaimTypes.Name, $"{user.FirstName} {user.LastName}"),
                new Claim(ClaimTypes.Email, user.Email),
            };

            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_jwtSecret));
            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

            var token = new JwtSecurityToken(
                issuer: _issuer,
                audience: _audience,
                claims: claims,
                expires: DateTime.UtcNow.AddMinutes(60), // Token expires in 60 minutes
                signingCredentials: creds);

            return new JwtSecurityTokenHandler().WriteToken(token);
        }

        private string GenerateRefreshToken()
        {
            var randomNumber = new byte[32];
            using var rng = RandomNumberGenerator.Create();
            rng.GetBytes(randomNumber);
            return Convert.ToBase64String(randomNumber);
        }

        private void ValidateSignUpRequest(SignUpRequestDto dto)
        {
            if (dto == null)
                throw new ArgumentNullException(nameof(dto), "Sign-up request cannot be null.");

            if (string.IsNullOrWhiteSpace(dto.Email) || !IsValidEmail(dto.Email))
                throw new ArgumentException("Invalid or missing email format.");

            if (string.IsNullOrWhiteSpace(dto.Password) || dto.Password.Length < 8 || dto.Password.Length > 20)
                throw new ArgumentException("Password must be between 8 and 20 characters.");
        }

        public static async Task<string> HashPasswordAsync(string password)
        {
            byte[] passwordBytes = Encoding.UTF8.GetBytes(password);
            var argon2 = new Argon2id(passwordBytes)
            {
                MemorySize = 4096,
                Iterations = 3,
                DegreeOfParallelism = 1,
                Salt = new byte[16]
            };
            RandomNumberGenerator.Fill(argon2.Salt);
            byte[] hash = await argon2.GetBytesAsync(32);
            return $"argon2id$v=19$m={argon2.MemorySize},t={argon2.Iterations},p={argon2.DegreeOfParallelism}${Convert.ToBase64String(argon2.Salt)}${Convert.ToBase64String(hash)}";
        }

        private static bool IsValidEmail(string email)
        {
            return System.Net.Mail.MailAddress.TryCreate(email, out _);
        }

        public static async Task<bool> VerifyPasswordAsync(string password, string hashedPassword)
        {
            try
            {
                // Validate inputs
                if (string.IsNullOrWhiteSpace(password) || string.IsNullOrWhiteSpace(hashedPassword))
                    return false;

                string[] parts = hashedPassword.Split('$');
                if (parts.Length != 5 || parts[0] != "argon2id" || parts[1] != "v=19")
                    return false;

                string[] paramParts = parts[2].Split(',');
                if (paramParts.Length != 3)
                    return false;

                int memorySize = int.Parse(paramParts[0].Substring(2)); // Skip "m="
                int iterations = int.Parse(paramParts[1].Substring(2)); // Skip "t="
                int parallelism = int.Parse(paramParts[2].Substring(2)); // Skip "p="

                byte[] salt = Convert.FromBase64String(parts[3]);
                byte[] expectedHash = Convert.FromBase64String(parts[4]);

                byte[] passwordBytes = Encoding.UTF8.GetBytes(password);
                var argon2 = new Argon2id(passwordBytes)
                {
                    MemorySize = memorySize,
                    Iterations = iterations,
                    DegreeOfParallelism = parallelism,
                    Salt = salt
                };

                byte[] computedHash = await argon2.GetBytesAsync(32);

                return CryptographicOperations.FixedTimeEquals(computedHash, expectedHash);
            }
            catch (Exception)
            {
                return false;
            }
        }
    }
}
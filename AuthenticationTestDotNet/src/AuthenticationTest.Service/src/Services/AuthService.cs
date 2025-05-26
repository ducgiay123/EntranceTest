using AuthenticationTest.Core.src.RepoAbstract;
using AuthenticationTest.Service.src.Abstracts;
using AuthenticationTest.Service.src.DTOs;
using AuthenticationTest.Core.src.Entities;
using Konscious.Security.Cryptography;
using Microsoft.IdentityModel.Tokens;
using System;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using System.Security.Claims;
using Microsoft.Extensions.Configuration;
using AutoMapper;
using AuthenticationTest.Core.RepoAbstract;
using AuthenticationTest.src.Core.Entities;

namespace AuthenticationTest.Service.src.Services
{
    public class AuthService : IAuthService
    {
        private readonly IUserRepository _userRepository;
        private readonly IMapper _mapper;
        private readonly string _jwtSecret;
        private readonly string _issuer;
        private readonly string _audience;

        public AuthService(IUserRepository userRepository, IMapper mapper, IConfiguration configuration)
        {
            _userRepository = userRepository ?? throw new ArgumentNullException(nameof(userRepository));
            _mapper = mapper ?? throw new ArgumentNullException(nameof(mapper));
            _jwtSecret = configuration["Jwt:Secret"] ?? throw new ArgumentNullException("Jwt:Secret is missing in configuration.");
            _issuer = configuration["Jwt:Issuer"] ?? "AuthenticationTest";
            _audience = configuration["Jwt:Audience"] ?? "AuthenticationTest";
        }

        public async Task<SignUpResponseDto> SignUpAsync(SignUpRequestDto dto)
        {
            if (!await _userRepository.IsEmailAvailableAsync(dto.Email))
                throw new InvalidOperationException("Email is already registered.");

            var user = _mapper.Map<Users>(dto);
            user.Hash = HashPasswordSync(dto.Password);
            user.CreatedAt = DateTime.UtcNow;
            user.UpdatedAt = DateTime.UtcNow;

            var createdUser = await _userRepository.CreateAsync(user);
            return _mapper.Map<SignUpResponseDto>(createdUser);
        }

        public async Task<LoginResponseDto> LoginAsync(LoginRequestDto dto)
        {
            var user = await _userRepository.GetByEmailAsync(dto.Email);
            if (user == null || string.IsNullOrWhiteSpace(user.Hash))
                throw new UnauthorizedAccessException("Cannot find user with this email.");

            bool isPasswordValid = await VerifyPasswordAsync(dto.Password, user.Hash);
            if (!isPasswordValid)
                throw new UnauthorizedAccessException("Invalid email or password.");

            string jwtToken = GenerateJwtToken(user);
            string refreshToken = GenerateRefreshToken();

            await _userRepository.StoreRefreshTokenAsync(user.Id, refreshToken);

            return new LoginResponseDto
            {
                User = _mapper.Map<UserDto>(user),
                Token = jwtToken,
                RefreshToken = refreshToken
            };
        }

        public async Task<bool> LogoutAsync(string email)
        {
            if (string.IsNullOrWhiteSpace(email))
                return false;
            return await _userRepository.RevokeAllRefreshTokensAsync(email);
        }

        public async Task<Users?> GetUserAsync(string email)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(email))
                {
                    Console.WriteLine("Invalid or missing email.");
                    return null;
                }

                // Fetch user from repository
                var user = await _userRepository.GetByEmailAsync(email);
                if (user == null)
                {
                    Console.WriteLine($"User with email {email} not found.");
                    return null;
                }

                return user;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error fetching user: {ex.Message}");
                return null;
            }
        }

        public async Task<RefreshTokenResponseDto> RefreshTokenAsync(string refreshToken)
        {
            if (string.IsNullOrWhiteSpace(refreshToken))
                throw new ArgumentException("Refresh token is required.");

            var user = await _userRepository.ValidateRefreshTokenAsync(refreshToken);
            if (user == null)
                throw new KeyNotFoundException("Invalid or expired refresh token.");

            await _userRepository.InvalidateRefreshTokenAsync(user.Id, refreshToken);

            string newJwtToken = GenerateJwtToken(user);
            string newRefreshToken = GenerateRefreshToken();

            await _userRepository.StoreRefreshTokenAsync(user.Id, newRefreshToken);

            return new RefreshTokenResponseDto
            {
                Token = newJwtToken,
                RefreshToken = newRefreshToken
            };
        }

        private string GenerateJwtToken(Users user)
        {
            var claims = new[]
            {
                new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
                new Claim(JwtRegisteredClaimNames.Sub, user.Email),
                new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
                new Claim(ClaimTypes.Name, $"{user.FirstName} {user.LastName}"),
                new Claim(ClaimTypes.Email, user.Email),
                new Claim(ClaimTypes.Role, user.Role.ToString())
            };

            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_jwtSecret));
            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

            var token = new JwtSecurityToken(
                issuer: _issuer,
                audience: _audience,
                claims: claims,
                expires: DateTime.UtcNow.AddMinutes(60),
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

        public static string HashPasswordSync(string password)
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

            // Block until the async hashing is complete
            // .GetAwaiter().GetResult() is generally preferred over .Result for avoiding deadlocks
            byte[] hash = argon2.GetBytesAsync(32).GetAwaiter().GetResult();

            return $"argon2id$v=19$m={argon2.MemorySize},t={argon2.Iterations},p={argon2.DegreeOfParallelism}${Convert.ToBase64String(argon2.Salt)}${Convert.ToBase64String(hash)}";
        }

        private static async Task<bool> VerifyPasswordAsync(string password, string hashedPassword)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(password) || string.IsNullOrWhiteSpace(hashedPassword))
                    return false;

                string[] parts = hashedPassword.Split('$');
                if (parts.Length != 5 || parts[0] != "argon2id" || parts[1] != "v=19")
                    return false;

                string[] paramParts = parts[2].Split(',');
                if (paramParts.Length != 3)
                    return false;

                int memorySize = int.Parse(paramParts[0].Substring(2));
                int iterations = int.Parse(paramParts[1].Substring(2));
                int parallelism = int.Parse(paramParts[2].Substring(2));

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
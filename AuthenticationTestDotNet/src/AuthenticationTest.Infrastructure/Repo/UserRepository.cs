using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AuthenticationTest.Core.RepoAbstract;
using AuthenticationTest.Infrastructure.Database;
using AuthenticationTest.src.Core.Entities;
using Microsoft.EntityFrameworkCore;

namespace AuthenticationTest.Infrastructure.Repo
{
    public class UserRepository : IUserRepository
    {
        private readonly AuthDbContext _context;

        public UserRepository(AuthDbContext context)
        {
            _context = context;
        }

        public async Task<User?> GetByEmailAsync(string email)
        {
            return await _context.User
                .Include(u => u.Tokens)
                .FirstOrDefaultAsync(u => u.Email == email);
        }

        public async Task<User> CreateAsync(User user)
        {
            _context.User.Add(user);
            await _context.SaveChangesAsync();
            return user;
        }

        public async Task<bool> IsEmailAvailableAsync(string email)
        {
            return !await _context.User.AnyAsync(u => u.Email == email);
        }
        public async Task StoreRefreshTokenAsync(int userId, string refreshToken)
        {
            // Verify user exists
            var user = await _context.User.FindAsync(userId);
            if (user == null)
                throw new ArgumentException($"User with ID {userId} not found.");

            var token = new Token
            {
                UserId = userId,
                RefreshToken = refreshToken,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow,
                ExpiresIn = DateTime.UtcNow.AddDays(30).ToString(), // 30-day expiry

            };

            _context.Token.Add(token);
            await _context.SaveChangesAsync();
        }
        public async Task<bool> RevokeRefreshTokenAsync(string email, string refreshToken)
        {
            // Find user by email
            var user = await _context.User
                .FirstOrDefaultAsync(u => u.Email == email);
            if (user == null)
                return false;

            int userId = user.Id;

            // Verify the provided refresh token is valid and non-expired
            var token = await _context.Token
                .FirstOrDefaultAsync(rt => rt.UserId == userId && rt.RefreshToken == refreshToken);

            if (token == null)
                return false;

            // Delete all non-expired tokens for the user
            var tokens = await _context.Token
                .Where(rt => rt.UserId == userId)
                .ToListAsync();

            if (!tokens.Any())
                return false;

            _context.Token.RemoveRange(tokens);
            await _context.SaveChangesAsync();
            return true;
        }
        public async Task<User?> ValidateRefreshTokenAsync(string refreshToken)
        {
            var token = await _context.Token
                .Include(rt => rt.User)
                .FirstOrDefaultAsync(rt => rt.RefreshToken == refreshToken);

            if (token == null)
                return null;

            return token.User;
        }

        public async Task InvalidateRefreshTokenAsync(int userId, string refreshToken)
        {
            var token = await _context.Token
                .FirstOrDefaultAsync(rt => rt.UserId == userId && rt.RefreshToken == refreshToken);

            if (token != null)
            {
                _context.Token.Remove(token);
                await _context.SaveChangesAsync();
            }
        }

    }
}
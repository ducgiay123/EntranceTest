using AuthenticationTest.src.Core.Entities;

namespace AuthenticationTest.Core.RepoAbstract;

public interface IUserRepository
{
    Task<User?> GetByEmailAsync(string email);
    Task<User> CreateAsync(User user);
    Task<bool> IsEmailAvailableAsync(string email);
    Task StoreRefreshTokenAsync(int userId, string refreshToken);
    Task<bool> RevokeRefreshTokenAsync(string email, string refreshToken);
    Task<User?> ValidateRefreshTokenAsync(string refreshToken);
    Task InvalidateRefreshTokenAsync(int userId, string refreshToken);

}

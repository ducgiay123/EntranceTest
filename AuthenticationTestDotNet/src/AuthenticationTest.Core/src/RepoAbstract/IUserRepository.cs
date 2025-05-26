using AuthenticationTest.src.Core.Entities;

namespace AuthenticationTest.Core.RepoAbstract;

public interface IUserRepository
{
    Task<Users?> GetByEmailAsync(string email);
    Task<Users> CreateAsync(Users user);
    Task<bool> IsEmailAvailableAsync(string email);
    Task StoreRefreshTokenAsync(int userId, string refreshToken);
    Task<IEnumerable<Users>> GetAllUserAsync();
    Task<bool> RevokeAllRefreshTokensAsync(string Email);
    Task<Users?> ValidateRefreshTokenAsync(string refreshToken);
    Task InvalidateRefreshTokenAsync(int userId, string refreshToken);
    Task<bool> DeleteAsync(string email);
    Task<Users> UpdateAsync(Users user);

}

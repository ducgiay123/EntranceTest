using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AuthenticationTest.Service.src.DTOs;
using AuthenticationTest.src.Core.Entities;

namespace AuthenticationTest.Service.src.Abstracts
{
    public interface IAuthService
    {
        Task<SignUpResponseDto> SignUpAsync(SignUpRequestDto dto);
        Task<LoginResponseDto> LoginAsync(LoginRequestDto dto);
        Task<bool> LogoutAsync(string email, string refreshToken);
        Task<User?> GetUserByEmailAsync(string email);
        Task<RefreshTokenResponseDto> RefreshTokenAsync(string refreshToken);
    }
}

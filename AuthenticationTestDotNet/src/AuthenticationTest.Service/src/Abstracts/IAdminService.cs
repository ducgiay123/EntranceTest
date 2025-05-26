using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AuthenticationTest.src.Core.Entities;
using static AuthenticationTest.Service.src.DTOs.UserDTO;

namespace AuthenticationTest.Service.src.Abstracts
{
    public interface IAdminService
    {
        Task<IEnumerable<UserReadDto>> GetAllUsersAsync();
        Task<UserReadDto?> GetUserByEmailAsync(string email);
        Task<bool> DeleteUserAsync(string email);
        Task<UserReadDto?> UpdateUserAsync(UpdateUserDto dto);
    }
}
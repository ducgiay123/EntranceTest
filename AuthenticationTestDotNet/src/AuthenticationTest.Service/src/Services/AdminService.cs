using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AuthenticationTest.Core.RepoAbstract;
using AuthenticationTest.Service.src.Abstracts;
using AuthenticationTest.src.Core.Entities;
using AutoMapper;
using static AuthenticationTest.Service.src.DTOs.UserDTO;

namespace AuthenticationTest.Service.src.Services
{
    public class AdminService : IAdminService
    {
        private readonly IUserRepository _userRepository;
        private readonly IMapper _mapper;
        public AdminService(IUserRepository userRepository, IMapper mapper)
        {
            _userRepository = userRepository;
            _mapper = mapper ?? throw new ArgumentNullException(nameof(mapper));
        }

        public async Task<IEnumerable<UserReadDto>> GetAllUsersAsync() // <-- Changed return type
        {
            try
            {
                IEnumerable<Users> users = await _userRepository.GetAllUserAsync();

                // Use AutoMapper to map the collection
                var userDtos = _mapper.Map<IEnumerable<UserReadDto>>(users); // Maps IEnumerable<Users> to IEnumerable<UserReadDto>

                return userDtos;
            }
            catch (Exception ex)
            {
                // Log the exception
                throw new ApplicationException("Error retrieving users.", ex);
            }
        }

        public async Task<UserReadDto?> GetUserByEmailAsync(string email) // <-- Changed return type
        {
            try
            {
                var user = await _userRepository.GetByEmailAsync(email);

                if (user == null)
                {
                    return null;
                    // Or if you prefer to throw for 'not found':
                    // throw new KeyNotFoundException($"User with email '{email}' not found.");
                }

                // Use AutoMapper to map the single entity
                var userDto = _mapper.Map<UserReadDto>(user);

                return userDto;
            }
            catch (Exception ex)
            {
                // Log the exception
                throw new ApplicationException($"Error retrieving user with email '{email}'.", ex);
            }
        }

        public async Task<bool> DeleteUserAsync(string email)
        {
            try
            {
                var user = await _userRepository.GetByEmailAsync(email);
                if (user == null)
                    throw new KeyNotFoundException($"Cannot delete. User with email '{email}' not found.");

                var result = await _userRepository.DeleteAsync(email);
                return result;
            }
            catch (Exception ex)
            {
                throw new ApplicationException($"Error deleting user with email '{email}'.", ex);
            }
        }

        public async Task<UserReadDto?> UpdateUserAsync(UpdateUserDto dto)
        {
            try
            {
                // 1. Retrieve the existing user entity from the database
                var existingUser = await _userRepository.GetByEmailAsync(dto.Email);

                // 2. Check if the user exists
                if (existingUser == null)
                {
                    // If not found, throw a specific exception for clarity
                    throw new KeyNotFoundException($"Cannot update. User with email '{dto.Email}' not found.");
                }

                // 3. Use AutoMapper to apply updates from the DTO to the existing entity.
                // This will update properties like FirstName and LastName.
                // Ensure you have a mapping configured from UpdateUserDto to Users in your AutoMapper profile.
                _mapper.Map(dto, existingUser);

                // 4. Manually update the UpdatedAt timestamp (or ensure it's handled in your AutoMapper profile)
                existingUser.UpdatedAt = DateTime.UtcNow;

                // 5. Persist the changes via the repository.
                // Assuming _userRepository.UpdateAsync returns the *updated* Users entity.
                var updatedUserEntity = await _userRepository.UpdateAsync(existingUser);

                // 6. Map the updated Users entity back to a UserReadDto to return
                // This ensures you're returning the correct DTO type with only allowed fields.
                var userReadDto = _mapper.Map<UserReadDto>(updatedUserEntity);

                return userReadDto;
            }
            catch (KeyNotFoundException)
            {
                // Re-throw KeyNotFoundException directly, as it's a specific business error
                throw;
            }
            catch (Exception ex)
            {
                // Catch any other general exceptions, log them, and throw a custom ApplicationException
                // It's good practice to log 'ex' here using an ILogger
                throw new ApplicationException($"Error updating user with email '{dto.Email}'.", ex);
            }
        }

    }
}

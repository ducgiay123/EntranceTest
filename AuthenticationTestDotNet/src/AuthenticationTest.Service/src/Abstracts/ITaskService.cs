using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AuthenticationTest.Core.src.Entities;
using AuthenticationTest.Service.src.DTOs;

namespace AuthenticationTest.Service.src.Abstracts
{
    public interface ITaskService
    {
        Task<IEnumerable<TaskDto>> GetAllAsync();
        Task<IEnumerable<TaskDto>> GetAllByUserIdAsync(int userId);
        Task<TaskDto?> GetByIdAsync(int id);
        Task<TaskDto?> GetByIdAndUserIdAsync(int id, int userId);
        Task<TaskDto> CreateAsync(CreateTaskDto dto, int userId);
        Task<bool> UpdateAsync(UpdateTaskDto dto, int userId);
        Task<bool> DeleteAsync(int id, int userId);
    }
}
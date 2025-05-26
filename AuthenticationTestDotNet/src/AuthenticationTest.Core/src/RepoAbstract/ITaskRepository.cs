using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AuthenticationTest.Core.src.Entities;

namespace AuthenticationTest.Core.src.RepoAbstract
{
    public interface ITaskRepository
    {
        Task<IEnumerable<Tasks>> GetAllAsync();
        Task<IEnumerable<Tasks>> GetAllByUserIdAsync(int userId);
        Task<Tasks?> GetByIdAsync(int id);
        Task<Tasks?> GetByIdAndUserIdAsync(int id, int userId);
        Task<Tasks> AddAsync(Tasks task);
        Task UpdateAsync(Tasks task);
        Task DeleteAsync(Tasks task);
        Task<bool> ExistsAsync(int id);
    }
}
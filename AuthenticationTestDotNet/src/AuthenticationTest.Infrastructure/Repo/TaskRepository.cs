using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AuthenticationTest.Core.src.Entities;
using AuthenticationTest.Core.src.RepoAbstract;
using AuthenticationTest.Infrastructure.Database;
using Microsoft.EntityFrameworkCore;

namespace AuthenticationTest.Infrastructure.Repo
{
    public class TaskRepository : ITaskRepository
    {
        private readonly AuthDbContext _context;

        public TaskRepository(AuthDbContext context)
        {
            _context = context;
        }

        public async Task<IEnumerable<Tasks>> GetAllAsync()
        {
            return await _context.Tasks.ToListAsync();
        }

        public async Task<Tasks?> GetByIdAsync(int id)
        {
            return await _context.Tasks.FindAsync(id);
        }

        public async Task<Tasks> AddAsync(Tasks task)
        {
            _context.Tasks.Add(task);
            await _context.SaveChangesAsync();
            return task;
        }

        public async Task UpdateAsync(Tasks task)
        {
            _context.Tasks.Update(task);
            await _context.SaveChangesAsync();
        }

        public async Task DeleteAsync(Tasks task)
        {
            _context.Tasks.Remove(task);
            await _context.SaveChangesAsync();
        }

        public async Task<bool> ExistsAsync(int id)
        {
            return await _context.Tasks.AnyAsync(t => t.Id == id);
        }
        public async Task<IEnumerable<Tasks>> GetAllByUserIdAsync(int userId)
        {
            if (userId <= 0)
                throw new ArgumentException("User ID must be greater than zero.", nameof(userId));

            return await _context.Tasks
                .Where(t => t.UserId == userId)
                .ToListAsync();
        }
        public async Task<Tasks?> GetByIdAndUserIdAsync(int id, int userId)
        {
            if (id <= 0)
                throw new ArgumentException("Task ID must be greater than zero.", nameof(id));
            if (userId <= 0)
                throw new ArgumentException("User ID must be greater than zero.", nameof(userId));

            return await _context.Tasks
                .FirstOrDefaultAsync(t => t.Id == id && t.UserId == userId);
        }
    }
}
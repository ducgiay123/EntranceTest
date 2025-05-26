using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using AutoMapper;
using AutoMapper.Internal;
using AuthenticationTest.Core.src.Entities;
using AuthenticationTest.Core.src.Utilities;
using AuthenticationTest.Service.src.Abstracts;
using AuthenticationTest.Service.src.DTOs;
using AuthenticationTest.Core.src.RepoAbstract;

namespace AuthenticationTest.Service.src.Services
{
    public class TaskService : ITaskService
    {
        private readonly ITaskRepository _repository;
        private readonly IMapper _mapper;

        public TaskService(ITaskRepository repository, IMapper mapper)
        {
            _repository = repository ?? throw new ArgumentNullException(nameof(repository));
            _mapper = mapper ?? throw new ArgumentNullException(nameof(mapper));
        }

        public async Task<IEnumerable<TaskDto>> GetAllAsync()
        {
            try
            {
                var tasks = await _repository.GetAllAsync();
                return _mapper.Map<IEnumerable<TaskDto>>(tasks);
            }
            catch (AutoMapperMappingException ex)
            {
                throw new Exception("Failed to map tasks to DTOs.", ex);
            }
            catch (Exception ex)
            {
                throw new Exception("Failed to retrieve tasks.", ex);
            }
        }

        public async Task<IEnumerable<TaskDto>> GetAllByUserIdAsync(int userId)
        {
            try
            {
                if (userId <= 0)
                    throw new ArgumentException("User ID must be greater than zero.", nameof(userId));

                var tasks = await _repository.GetAllByUserIdAsync(userId);
                return _mapper.Map<IEnumerable<TaskDto>>(tasks);
            }
            catch (AutoMapperMappingException ex)
            {
                throw new Exception("Failed to map tasks to DTOs.", ex);
            }
            catch (Exception ex)
            {
                throw new Exception($"Failed to retrieve tasks for user ID {userId}.", ex);
            }
        }

        public async Task<TaskDto?> GetByIdAsync(int id)
        {
            try
            {
                if (id <= 0)
                    throw new ArgumentException("Task ID must be greater than zero.", nameof(id));

                var task = await _repository.GetByIdAsync(id);
                if (task == null)
                    return null;

                var taskDto = _mapper.Map<TaskDto>(task);
                if (taskDto == null)
                    throw new InvalidOperationException($"AutoMapper returned null when mapping task with ID {id} to TaskDto.");

                return taskDto;
            }
            catch (AutoMapperMappingException ex)
            {
                throw new InvalidOperationException($"AutoMapper failed to map task with ID {id} to TaskDto.", ex);
            }
            catch (ArgumentException)
            {
                throw; // Re-throw for controller to handle as 400 Bad Request
            }
            catch (InvalidOperationException)
            {
                throw; // Re-throw mapping-specific errors
            }
            catch (Exception ex)
            {
                throw new Exception($"Failed to retrieve task with ID {id} from the repository.", ex);
            }
        }

        public async Task<TaskDto?> GetByIdAndUserIdAsync(int id, int userId)
        {
            try
            {
                if (id <= 0)
                    throw new ArgumentException("Task ID must be greater than zero.", nameof(id));
                if (userId <= 0)
                    throw new ArgumentException("User ID must be greater than zero.", nameof(userId));

                var task = await _repository.GetByIdAndUserIdAsync(id, userId);
                if (task == null)
                    return null;

                var taskDto = _mapper.Map<TaskDto>(task);
                if (taskDto == null)
                    throw new InvalidOperationException($"AutoMapper returned null when mapping task with ID {id} to TaskDto.");

                return taskDto;
            }
            catch (AutoMapperMappingException ex)
            {
                throw new InvalidOperationException($"AutoMapper failed to map task with ID {id} to TaskDto.", ex);
            }
            catch (ArgumentException)
            {
                throw; // Re-throw for controller to handle as 400 Bad Request
            }
            catch (InvalidOperationException)
            {
                throw; // Re-throw mapping-specific errors
            }
            catch (Exception ex)
            {
                throw new Exception($"Failed to retrieve task with ID {id} for user ID {userId} from the repository.", ex);
            }
        }

        public async Task<TaskDto> CreateAsync(CreateTaskDto dto, int userId)
        {
            try
            {
                if (dto == null)
                    throw new ArgumentNullException(nameof(dto));
                if (userId <= 0)
                    throw new ArgumentException("User ID must be greater than zero.", nameof(userId));

                // Validate Status explicitly
                if (!Enum.TryParse<TasksStatus>(dto.Status, true, out var parsedStatus))
                    throw new ArgumentException($"Invalid Status value: {dto.Status}. Must be one of: {string.Join(", ", Enum.GetNames(typeof(TasksStatus)))}");

                var task = _mapper.Map<Tasks>(dto);
                task.UserId = userId; // Set UserId from the provided parameter
                var created = await _repository.AddAsync(task);
                return _mapper.Map<TaskDto>(created);
            }
            catch (AutoMapperMappingException ex)
            {
                throw new Exception("Failed to map CreateTaskDto to Tasks entity.", ex);
            }
            catch (ArgumentException)
            {
                throw; // Re-throw validation errors for controller to handle
            }
            catch (Exception ex)
            {
                throw new Exception("Failed to create task.", ex);
            }
        }

        public async Task<bool> UpdateAsync(UpdateTaskDto dto, int userId)
        {
            try
            {
                if (dto == null)
                    return false; // Return false to trigger 400 Bad Request in controller
                if (dto.Id <= 0)
                    throw new ArgumentException("Invalid task ID.", nameof(dto.Id));
                if (userId <= 0)
                    throw new ArgumentException("User ID must be greater than zero.", nameof(userId));

                var existing = await _repository.GetByIdAndUserIdAsync(dto.Id, userId);
                if (existing == null)
                    return false;

                // Update only provided fields
                if (!string.IsNullOrEmpty(dto.Title))
                    existing.Title = dto.Title;

                if (!string.IsNullOrEmpty(dto.Description))
                    existing.Description = dto.Description;

                if (!string.IsNullOrEmpty(dto.Status))
                {
                    if (!Enum.TryParse<TasksStatus>(dto.Status, true, out var parsedStatus))
                        throw new ArgumentException($"Invalid Status value: {dto.Status}. Must be one of: {string.Join(", ", Enum.GetNames(typeof(TasksStatus)))}");
                    existing.Status = parsedStatus;
                }

                if (dto.DueDate.HasValue)
                    existing.DueDate = dto.DueDate.Value;

                await _repository.UpdateAsync(existing);
                return true;
            }
            catch (AutoMapperMappingException ex)
            {
                throw new Exception($"Failed to map UpdateTaskDto to Tasks entity for ID {dto.Id}.", ex);
            }
            catch (Exception ex)
            {
                throw new Exception($"Failed to update task with ID {dto.Id}.", ex);
            }
        }

        public async Task<bool> DeleteAsync(int id, int userId)
        {
            try
            {
                if (id <= 0)
                    throw new ArgumentException("Invalid task ID.", nameof(id));
                if (userId <= 0)
                    throw new ArgumentException("User ID must be greater than zero.", nameof(userId));

                var existing = await _repository.GetByIdAndUserIdAsync(id, userId);
                if (existing == null)
                    return false;

                await _repository.DeleteAsync(existing);
                return true;
            }
            catch (Exception ex)
            {
                throw new Exception($"Failed to delete task with ID {id}.", ex);
            }
        }
    }
}
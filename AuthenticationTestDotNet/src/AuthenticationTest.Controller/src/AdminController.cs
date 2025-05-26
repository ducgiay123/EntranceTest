using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using AuthenticationTest.Service.src.Abstracts;
using AuthenticationTest.Service.src.DTOs;
using AuthenticationTest.src.Core.Entities; // Assuming 'Users' entity is still needed for some operations
using System; // Needed for ArgumentException, KeyNotFoundException etc.
using System.Collections.Generic; // Needed for IEnumerable
using Microsoft.Extensions.Logging;
using static AuthenticationTest.Service.src.DTOs.UserDTO; // Added for logging exceptions

namespace AuthenticationTest.API.Controllers
{
    [ApiController]
    [Route("api/v1/admin")]
    [Authorize(Roles = "Admin")] // Only admins can access
    public class AdminController : ControllerBase
    {
        private readonly IAdminService _adminService;
        private readonly ITaskService _taskService; // Injected TaskService
        private readonly ILogger<AdminController> _logger; // Injected Logger

        public AdminController(IAdminService adminService, ITaskService taskService, ILogger<AdminController> logger)
        {
            _adminService = adminService ?? throw new ArgumentNullException(nameof(adminService));
            _taskService = taskService ?? throw new ArgumentNullException(nameof(taskService));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        // --- User Management Endpoints ---

        [HttpGet("users")]
        public async Task<ActionResult<IEnumerable<UserReadDto>>> GetAllUsers() // Changed to UserReadDto
        {
            try
            {
                var users = await _adminService.GetAllUsersAsync();
                return Ok(users);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "AdminController: Error getting all users.");
                return StatusCode(500, "An internal server error occurred.");
            }
        }

        [HttpGet("users/{email}")]
        public async Task<ActionResult<UserReadDto>> GetUserByEmail(string email) // Changed to UserReadDto
        {
            try
            {
                var user = await _adminService.GetUserByEmailAsync(email);
                if (user == null)
                    return NotFound($"User with email '{email}' not found.");

                return Ok(user);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "AdminController: Error getting user by email '{Email}'.", email);
                return StatusCode(500, "An internal server error occurred.");
            }
        }

        [HttpPut("users/update")]
        public async Task<ActionResult<UserReadDto>> UpdateUser([FromBody] UpdateUserDto dto) // Changed to UserReadDto
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            try
            {
                var updatedUser = await _adminService.UpdateUserAsync(dto);
                // The service might return null if not found, depending on its implementation
                if (updatedUser == null)
                {
                    return NotFound($"User with email '{dto.Email}' not found.");
                }
                return Ok(updatedUser);
            }
            catch (KeyNotFoundException ex) // Catches specific "not found" from service
            {
                return NotFound(ex.Message);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "AdminController: Error updating user with email '{Email}'.", dto.Email);
                return StatusCode(500, "An internal server error occurred during user update.");
            }
        }

        [HttpDelete("users/{email}")]
        public async Task<ActionResult> DeleteUser(string email)
        {
            try
            {
                var result = await _adminService.DeleteUserAsync(email);
                if (!result)
                    return NotFound($"User with email '{email}' not found or could not be deleted.");

                return NoContent();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "AdminController: Error deleting user with email '{Email}'.", email);
                return StatusCode(500, "An internal server error occurred during user deletion.");
            }
        }


        [HttpGet("tasks")]
        public async Task<ActionResult<IEnumerable<TaskDto>>> GetAllTasks()
        {
            try
            {
                var tasks = await _taskService.GetAllAsync();
                return Ok(tasks);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "AdminController: Error getting all tasks.");
                return StatusCode(500, "An unexpected error occurred while retrieving all tasks.");
            }
        }

        [HttpGet("tasks/user/{userId}")]
        public async Task<ActionResult<IEnumerable<TaskDto>>> GetAllTasksByUserId(int userId)
        {
            try
            {
                var tasks = await _taskService.GetAllByUserIdAsync(userId);
                return Ok(tasks);
            }
            catch (ArgumentException ex)
            {
                _logger.LogWarning(ex, "AdminController: Invalid argument for GetAllTasksByUserId (UserId: {UserId}).", userId);
                return BadRequest(ex.Message);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "AdminController: Error getting tasks for user ID {UserId}.", userId);
                return StatusCode(500, $"An unexpected error occurred while retrieving tasks for user ID {userId}.");
            }
        }

        [HttpGet("tasks/{id}")]
        public async Task<ActionResult<TaskDto>> GetTaskById(int id)
        {
            try
            {
                var task = await _taskService.GetByIdAsync(id);
                if (task == null)
                {
                    return NotFound($"Task with ID {id} not found.");
                }
                return Ok(task);
            }
            catch (ArgumentException ex)
            {
                _logger.LogWarning(ex, "AdminController: Invalid argument for GetTaskById (Id: {Id}).", id);
                return BadRequest(ex.Message);
            }
            catch (InvalidOperationException ex)
            {
                _logger.LogError(ex, "AdminController: AutoMapper failed to map task with ID {Id}.", id);
                return StatusCode(500, "Error processing task data during retrieval.");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "AdminController: Error getting task by ID {Id}.", id);
                return StatusCode(500, $"An unexpected error occurred while retrieving task with ID {id}.");
            }
        }

        [HttpGet("tasks/{id}/user/{userId}")]
        public async Task<ActionResult<TaskDto>> GetTaskByIdAndUserId(int id, int userId)
        {
            try
            {
                var task = await _taskService.GetByIdAndUserIdAsync(id, userId);
                if (task == null)
                {
                    return NotFound($"Task with ID {id} for user ID {userId} not found.");
                }
                return Ok(task);
            }
            catch (ArgumentException ex)
            {
                _logger.LogWarning(ex, "AdminController: Invalid argument for GetTaskByIdAndUserId (Id: {Id}, UserId: {UserId}).", id, userId);
                return BadRequest(ex.Message);
            }
            catch (InvalidOperationException ex)
            {
                _logger.LogError(ex, "AdminController: AutoMapper failed to map task with ID {Id} for user ID {UserId}.", id, userId);
                return StatusCode(500, "Error processing task data.");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "AdminController: Error getting task by ID {Id} and user ID {UserId}.", id, userId);
                return StatusCode(500, $"An unexpected error occurred while retrieving task with ID {id} for user ID {userId}.");
            }
        }

        [HttpPost("tasks/user/{userId}")]
        public async Task<ActionResult<TaskDto>> CreateTask(int userId, [FromBody] CreateTaskDto createTaskDto)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return BadRequest(ModelState);
                }

                var createdTask = await _taskService.CreateAsync(createTaskDto, userId);
                return CreatedAtAction(nameof(GetTaskById), new { id = createdTask.Id }, createdTask);
            }
            catch (ArgumentException ex)
            {
                _logger.LogWarning(ex, "AdminController: Invalid argument for CreateTask (UserId: {UserId}).", userId);
                return BadRequest(ex.Message);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "AdminController: Error creating task for user ID {UserId}.", userId);
                return StatusCode(500, $"An unexpected error occurred while creating a task for user ID {userId}.");
            }
        }

        [HttpPut("tasks/{id}/user/{userId}")]
        public async Task<IActionResult> UpdateTask(int id, int userId, [FromBody] AdminUpdateTaskDto adminUpdateTaskDto)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return BadRequest(ModelState);
                }
                UpdateTaskDto updateTaskDto = new UpdateTaskDto
                {
                    Id = id,
                    Title = adminUpdateTaskDto.Title,
                    Description = adminUpdateTaskDto.Description,
                    Status = adminUpdateTaskDto.Status,
                    DueDate = adminUpdateTaskDto.DueDate
                };
                var success = await _taskService.UpdateAsync(updateTaskDto, userId);
                if (!success)
                {
                    return NotFound($"Task with ID {id} for user ID {userId} not found or not authorized.");
                }

                return NoContent();
            }
            catch (ArgumentException ex)
            {
                _logger.LogWarning(ex, "AdminController: Invalid argument for UpdateTask (Id: {Id}, UserId: {UserId}).", id, userId);
                return BadRequest(ex.Message);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "AdminController: Error updating task with ID {Id} for user ID {UserId}.", id, userId);
                return StatusCode(500, $"An unexpected error occurred while updating task with ID {id}.");
            }
        }

        [HttpDelete("tasks/{id}/user/{userId}")]
        public async Task<IActionResult> DeleteTask(int id, int userId)
        {
            try
            {
                var success = await _taskService.DeleteAsync(id, userId);
                if (!success)
                {
                    return NotFound($"Task with ID {id} for user ID {userId} not found or could not be deleted.");
                }
                return NoContent();
            }
            catch (ArgumentException ex)
            {
                _logger.LogWarning(ex, "AdminController: Invalid argument for DeleteTask (Id: {Id}, UserId: {UserId}).", id, userId);
                return BadRequest(ex.Message);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "AdminController: Error deleting task with ID {Id} for user ID {UserId}.", id, userId);
                return StatusCode(500, $"An unexpected error occurred while deleting task with ID {id}.");
            }
        }
    }
}
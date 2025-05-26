using System;
using System.Collections.Generic;
using System.Security.Claims;
using System.Threading.Tasks;
using AuthenticationTest.Core.src.Utilities;
using AuthenticationTest.Service.src.Abstracts;
using AuthenticationTest.Service.src.DTOs;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AuthenticationTest.Controller.src
{
    [Authorize]
    [ApiController]
    [Route("api/v1/tasks")]
    public class TaskController : ControllerBase
    {
        private readonly ITaskService _taskService;

        public TaskController(ITaskService taskService)
        {
            _taskService = taskService ?? throw new ArgumentNullException(nameof(taskService));
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<TaskDto>>> GetAll()
        {
            try
            {
                var userId = GetUserId();
                var tasks = await _taskService.GetAllByUserIdAsync(userId);
                return Ok(tasks);
            }
            catch (ArgumentException ex)
            {
                return BadRequest(new { Message = ex.Message });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { Message = "An error occurred while retrieving tasks.", Error = ex.Message });
            }
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<TaskDto>> GetById(int id)
        {
            try
            {
                var userId = GetUserId();
                var task = await _taskService.GetByIdAndUserIdAsync(id, userId);
                if (task == null)
                    return NotFound(new { Message = $"Task with ID {id} not found for user." });

                return Ok(task);
            }
            catch (ArgumentException ex)
            {
                return BadRequest(new { Message = ex.Message });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { Message = $"An error occurred while retrieving task with ID {id}.", Error = ex.Message });
            }
        }

        [HttpPost]
        public async Task<ActionResult<TaskDto>> Create([FromBody] CreateTaskDto dto)
        {
            try
            {
                if (!ModelState.IsValid)
                    return BadRequest(ModelState);

                var userId = GetUserId();
                var createdTask = await _taskService.CreateAsync(dto, userId);
                return CreatedAtAction(nameof(GetById), new { id = createdTask.Id }, createdTask);
            }
            catch (ArgumentNullException ex)
            {
                return BadRequest(new { Message = ex.Message });
            }
            catch (ArgumentException ex)
            {
                return BadRequest(new { Message = ex.Message });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { Message = "An error occurred while creating the task.", Error = ex.Message });
            }
        }

        [HttpPut]
        public async Task<ActionResult> Update([FromBody] UpdateTaskDto dto)
        {
            try
            {
                if (dto == null)
                    return BadRequest(new { Message = "Request body cannot be null." });

                if (!ModelState.IsValid)
                    return BadRequest(ModelState);

                if (dto.Status != null && !Enum.TryParse<TasksStatus>(dto.Status, true, out _))
                    return BadRequest(new { Message = $"Invalid Status value: {dto.Status}. Must be one of: {string.Join(", ", Enum.GetNames(typeof(TasksStatus)))}" });

                var userId = GetUserId();
                var result = await _taskService.UpdateAsync(dto, userId);
                if (!result)
                    return NotFound(new { Message = $"Task with ID {dto.Id} not found for user." });

                return NoContent();
            }
            catch (ArgumentException ex)
            {
                return BadRequest(new { Message = ex.Message });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { Message = $"An error occurred while updating task with ID {dto.Id}.", Error = ex.Message });
            }
        }

        [HttpDelete("{id}")]
        public async Task<ActionResult> Delete(int id)
        {
            try
            {
                var userId = GetUserId();
                var result = await _taskService.DeleteAsync(id, userId);
                if (!result)
                    return NotFound(new { Message = $"Task with ID {id} not found for user." });

                return NoContent();
            }
            catch (ArgumentException ex)
            {
                return BadRequest(new { Message = ex.Message });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { Message = $"An error occurred while deleting task with ID {id}.", Error = ex.Message });
            }
        }

        private int GetUserId()
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userIdClaim) || !int.TryParse(userIdClaim, out var userId) || userId <= 0)
                throw new ArgumentException("Invalid or missing user ID in authentication token.");
            return userId;
        }
    }
}
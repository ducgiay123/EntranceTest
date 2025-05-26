using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace AuthenticationTest.Service.src.DTOs
{
    public class CreateTaskDto
    {
        [Required]
        [MaxLength(100)]
        public string Title { get; set; } = string.Empty;
        [MaxLength(500)]
        public string? Description { get; set; }
        [Required]
        public string Status { get; set; } = "ToDo";
        [Required]
        public DateTime DueDate { get; set; }
    }
    public class UpdateTaskDto
    {
        [Required]
        public int Id { get; set; }

        [MaxLength(100)]
        public string? Title { get; set; } = string.Empty;

        [MaxLength(500)]
        public string? Description { get; set; }

        public string? Status { get; set; } = "ToDo";

        public DateTime? DueDate { get; set; }
    }
    public class AdminUpdateTaskDto
    {

        [MaxLength(100)]
        public string? Title { get; set; } = string.Empty;

        [MaxLength(500)]
        public string? Description { get; set; }

        public string? Status { get; set; } = "ToDo";

        public DateTime? DueDate { get; set; }
    }
    public class TaskDto
    {
        public int Id { get; set; }
        public string Title { get; set; } = string.Empty;
        public string? Description { get; set; }
        public string Status { get; set; } = "ToDo"; // Stored as string
        public DateTime DueDate { get; set; }
        public string UserId { get; set; } = string.Empty;
    }

}
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Threading.Tasks;
using AuthenticationTest.Core.src.Utilities;
using AuthenticationTest.src.Core.Entities;

namespace AuthenticationTest.Core.src.Entities
{
    public class Tasks
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "User ID is required.")]
        public int UserId { get; set; }

        // Navigation property to the User
        [ForeignKey("UserId")]
        public Users User { get; set; } = null!;

        [Required(ErrorMessage = "Title is required.")]
        [StringLength(100, ErrorMessage = "Title can't be longer than 100 characters.")]
        public string Title { get; set; } = string.Empty;

        [StringLength(500, ErrorMessage = "Description can't be longer than 500 characters.")]
        public string Description { get; set; } = string.Empty;

        [Required(ErrorMessage = "Status is required.")]
        public TasksStatus Status { get; set; }

        [Required(ErrorMessage = "Due date is required.")]
        [DataType(DataType.Date)]
        public DateTime DueDate { get; set; }
    }
}
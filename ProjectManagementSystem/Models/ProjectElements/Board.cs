using System;
using System.ComponentModel.DataAnnotations;

namespace ProjectManagementSystem.Models.ProjectElements
{
    public class Board
    {
        [Key]
        public int Id { get; set; }
        [Required]
        public Team team { get; set; }
        [Required]
        public Project project { get; set; }
        [Required]
        public DateTime startDate { get; set; } = DateTime.Now;
        public DateTime endDate { get; set; }
        public bool isFinished { get; set; } = false;

    }
}

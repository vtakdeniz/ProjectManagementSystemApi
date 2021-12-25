using System;
using System.ComponentModel.DataAnnotations;

namespace ProjectManagementSystem.Models.ProjectElements
{
    public class Project
    {
        [Key]
        public int Id { get; set; }
        [Required]
        public string projectName { get; set; }
        public string projectDescription { get; set; }
        [Required]
        public DateTime startDate { get; set; } = DateTime.Now;
        public DateTime endDate { get; set; }
        public bool isFinished { get; set; }
    }
}

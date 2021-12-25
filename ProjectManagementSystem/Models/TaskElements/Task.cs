using System;
using System.ComponentModel.DataAnnotations;
using ProjectManagementSystem.Models.ProjectElements;
using ProjectManagementSystem.Models.UserElements;

namespace ProjectManagementSystem.Models.TaskElements
{
    public class Task
    {
        [Key]
        public int Id { get; set; }
        public Section section { get; set; }
        [Required]
        public string taskTitle { get; set; }
        public string taskDescription { get; set; }
        public DateTime startDate { get; set; } = DateTime.Now;
        public DateTime endDate { get; set; }
        public bool isFinished { get; set; } = false;
        [Required]
        public User createUser { get; set; }
        public User receiverUser { get; set; }
        public Project project { get; set; }
    }
}

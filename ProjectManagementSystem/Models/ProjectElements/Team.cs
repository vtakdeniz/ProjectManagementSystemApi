using System;
using System.ComponentModel.DataAnnotations;

namespace ProjectManagementSystem.Models.ProjectElements
{
    public class Team
    {
        [Key]
        public int Id { get; set; }
        [Required]
        public string teamName { get; set; }
        [Required]
        public Project project { get; set; }
    }
}

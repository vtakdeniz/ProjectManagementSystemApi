using System;
using System.ComponentModel.DataAnnotations;

namespace ProjectManagementSystem.Dto.ProjectDto
{
    public class CreateChecklistDto
    {
        [Required]
        public int job_id { get; set; }
        [Required]
        public string text { get; set; }
        public bool isSelected { get; set; }
    }
}

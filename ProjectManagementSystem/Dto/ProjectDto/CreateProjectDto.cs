using System;
using System.ComponentModel.DataAnnotations;
using AutoMapper;

namespace ProjectManagementSystem.Dto.ProjectDto
{
    public class CreateProjectDto
    {
        [Required]
        public string projectName { get; set; }
        public string projectDescription { get; set; }
        [Required]
        public DateTime startDate { get; set; } = DateTime.Now;
        public DateTime endDate { get; set; }
        [IgnoreMap]
        public bool isFinished { get; set; } = false;
    }
}

using System;
using System.ComponentModel.DataAnnotations;
using ProjectManagementSystem.Dto.UserDto;
using ProjectManagementSystem.Dto.BoardReadDto;
using Newtonsoft.Json;
using System.Collections.Generic;
using ProjectManagementSystem.Dto.JobDto;

namespace ProjectManagementSystem.Dto.ProjectDto
{
    public class ReadProjectDto
    {
        [Key]
        public int Id { get; set; }
        [Required]
        public string projectName { get; set; }
        public string projectDescription { get; set; }
        [Required]
        public DateTime startDate { get; set; } = DateTime.Now;
        public DateTime endDate { get; set; } = DateTime.MaxValue;
        public bool isFinished { get; set; }
        
        public ReadUserDto assigner_user { get; set; }
        public List<ReadBoardDto> boards{ get; set; }
        public List<ReadUserDto> userHasProjects { get; set; }
        public List<ReadUserDto> userAssignedProjects { get; set; }
        public List<ReadJobDto> projectJobs { get; set; }
        public List<ReadTeamDto> teams { get; set; }
    }
}

using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using ProjectManagementSystem.Dto.UserDto;
using ProjectManagementSystem.Models.JobElements;
using ProjectManagementSystem.Models.ProjectElements;
using ProjectManagementSystem.Models.RelationTables;
using ProjectManagementSystem.Models.UserElements;

namespace ProjectManagementSystem.Dto.JobDto
{
    public class CreateJobDto
    {

        public int section_id { get; set; }
        [Required]
        public string jobTitle { get; set; }
        public string jobDescription { get; set; }
        public DateTime startDate { get; set; } = DateTime.Now;
        public DateTime endDate { get; set; }
        public string receiverUserId { get; set; }
        [Required]
        public int project_id { get; set; }
        public string[] tags { get; set; }
        
    }
}

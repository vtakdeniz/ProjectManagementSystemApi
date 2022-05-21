using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using ProjectManagementSystem.Dto.UserDto;

namespace ProjectManagementSystem.Dto.ProjectDto
{
    public class ReadTeamDto
    {
        [Key]
        public int Id { get; set; }
        [Required]
        public string teamName { get; set; }
        [Required]
        public int project_id { get; set; }

        public List<ReadUserDto> users { get; set; }
    }
}

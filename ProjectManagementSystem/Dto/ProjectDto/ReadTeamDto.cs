using System;
using System.ComponentModel.DataAnnotations;

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
    }
}

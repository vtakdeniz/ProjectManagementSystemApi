using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace ProjectManagementSystem.Dto.ProjectDto
{
    public class CreateTeamDto
    {
        [Required]
        public string teamName { get; set; }
        [Required]
        public int project_id { get; set; }

        public List<string> user_ids { get; set; }
    }
}

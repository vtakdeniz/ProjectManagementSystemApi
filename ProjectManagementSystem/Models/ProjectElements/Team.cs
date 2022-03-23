using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using ProjectManagementSystem.Models.RelationTables;

namespace ProjectManagementSystem.Models.ProjectElements
{
    public class Team
    {
        [Key]
        public int Id { get; set; }
        [Required]
        public string teamName { get; set; }
        [Required]
        public int project_id { get; set; }
        public Project project { get; set; }
        public List<BoardHasTeams> boardHasTeams { get; set; }
        public List<TeamHasUsers> teamHasUsers { get; set; }
    }
}
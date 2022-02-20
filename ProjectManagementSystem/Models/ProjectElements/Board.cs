using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using ProjectManagementSystem.Models.RelationTables;

namespace ProjectManagementSystem.Models.ProjectElements
{
    public class Board
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public int project_id { get; set; }
        public Project project { get; set; }

        [Required]
        public DateTime startDate { get; set; } = DateTime.Now;
        public DateTime endDate { get; set; }
        public bool isFinished { get; set; } = false;
        public List<Section> sections { get; set; }
        public List<BoardHasAdmins> boardHasAdmins { get; set; }
        public List<BoardHasTeams> boardHasTeams { get; set; }
        public List<BoardHasUsers> boardHasUsers{ get; set; }
    }
}

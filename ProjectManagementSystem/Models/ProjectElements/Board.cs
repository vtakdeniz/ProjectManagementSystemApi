using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using ProjectManagementSystem.Models.RelationTables;
using Newtonsoft.Json;

namespace ProjectManagementSystem.Models.ProjectElements
{
    public class Board
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public int project_id { get; set; }
        [JsonIgnore]
        public Project project { get; set; }
        [Required]
        public string board_name { get; set; }
        public string description { get; set; }
        public DateTime startDate { get; set; } = DateTime.Now;
        public DateTime endDate { get; set; } = DateTime.MaxValue;
        public bool isFinished { get; set; } = false;
        public List<Section> sections { get; set; }

        public List<BoardHasAdmins> boardHasAdmins { get; set; }
        [JsonIgnore]
        public List<BoardHasTeams> boardHasTeams { get; set; }
        [JsonIgnore]
        public List<BoardHasUsers> boardHasUsers{ get; set; }
    }
}

using System;
using ProjectManagementSystem.Models.ProjectElements;
using System.ComponentModel.DataAnnotations;

namespace ProjectManagementSystem.Models.RelationTables
{
    public class BoardHasTeams
    {
        public int team_id { get; set; }
        public Team team { get; set; }

        public int board_id { get; set; }
        public Board board { get; set; }

    }
}

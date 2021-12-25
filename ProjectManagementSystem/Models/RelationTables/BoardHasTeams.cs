using System;
using ProjectManagementSystem.Models.ProjectElements;
using System.ComponentModel.DataAnnotations;

namespace ProjectManagementSystem.Models.RelationTables
{
    public class BoardHasTeams
    {
        [Key]
        public Team team { get; set; }
        public Board board { get; set; }

    }
}

using System;
using ProjectManagementSystem.Models.UserElements;
using ProjectManagementSystem.Models.ProjectElements;
using System.ComponentModel.DataAnnotations;

namespace ProjectManagementSystem.Models.RelationTables
{
    public class TeamHasUsers
    {
        public string user_id { get; set; }
        public User user { get; set; }

        public int team_id { get; set; }
        public Team team { get; set; }
    }
}

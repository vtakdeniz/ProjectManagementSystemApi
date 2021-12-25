using System;
using ProjectManagementSystem.Models.UserElements;
using ProjectManagementSystem.Models.ProjectElements;
using System.ComponentModel.DataAnnotations;

namespace ProjectManagementSystem.Models.RelationTables
{
    public class TeamHasUsers
    {
        [Key]
        public User user { get; set; }
        [Key]
        public Team team { get; set; }
    }
}

using System;
using ProjectManagementSystem.Models.UserElements;
using ProjectManagementSystem.Models.ProjectElements;
using System.ComponentModel.DataAnnotations;

namespace ProjectManagementSystem.Models.RelationTables
{
    public class BoardHasUsers
    {
        [Key]
        public User user { get; set; }
        [Key]
        public Board board { get; set; }

    }
}

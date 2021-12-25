using System;
using ProjectManagementSystem.Models.UserElements;
using ProjectManagementSystem.Models.ProjectElements;
using System.ComponentModel.DataAnnotations;

namespace ProjectManagementSystem.Models.RelationTables
{
    public class BoardHasAdmins
    {
        [Key]
        public Board board { get; set; }
        [Key]
        public User user { get; set; }
    }
}

using System;
using ProjectManagementSystem.Models.UserElements;
using ProjectManagementSystem.Models.ProjectElements;
using System.ComponentModel.DataAnnotations;

namespace ProjectManagementSystem.Models.RelationTables
{
    public class UserHasProjects
    {
        [Key]
        public User user { get; set; }
        [Key]
        public Project project { get; set; }
        [Key]
        public int userSpaceProjectId { get; set; }
    }
}

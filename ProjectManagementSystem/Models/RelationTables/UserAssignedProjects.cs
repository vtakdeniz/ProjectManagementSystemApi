using System;
using ProjectManagementSystem.Models.UserElements;
using ProjectManagementSystem.Models.ProjectElements;
using System.ComponentModel.DataAnnotations;

namespace ProjectManagementSystem.Models.RelationTables
{
    public class UserAssignedProjects
    {
        [Key]
        public User receiverUser { get; set; }
        [Key]
        public Project project { get; set; }
        
        public User assignerUser { get; set; }
    }
}

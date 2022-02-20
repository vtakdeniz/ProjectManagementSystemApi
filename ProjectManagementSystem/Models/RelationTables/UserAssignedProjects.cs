using System;
using ProjectManagementSystem.Models.UserElements;
using ProjectManagementSystem.Models.ProjectElements;
using System.ComponentModel.DataAnnotations;

namespace ProjectManagementSystem.Models.RelationTables
{
    public class UserAssignedProjects
    {
        public string receiver_id { get; set; }
        public User receiverUser { get; set; }

        public int project_id { get; set; }
        public Project project { get; set; }

        public string assigner_id { get; set; }
        public User assignerUser { get; set; }
    }
}

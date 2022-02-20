using System;
using ProjectManagementSystem.Models.UserElements;
using ProjectManagementSystem.Models.ProjectElements;
using System.ComponentModel.DataAnnotations;

namespace ProjectManagementSystem.Models.RelationTables
{
    public class UserHasProjects
    {
        public string user_id { get; set; }
        public User user { get; set; }

        public int project_id { get; set; }
        public Project project { get; set; }

    }
}

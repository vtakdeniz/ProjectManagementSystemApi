using System;
using ProjectManagementSystem.Models.UserElements;
using ProjectManagementSystem.Models.JobElements;
using System.ComponentModel.DataAnnotations;

namespace ProjectManagementSystem.Models.RelationTables
{
    public class JobHasUsers
    {
        public string user_id { get; set; }
        public User user { get; set; }

        public int job_id { get; set; }
        public Job job { get; set; }
    }
}

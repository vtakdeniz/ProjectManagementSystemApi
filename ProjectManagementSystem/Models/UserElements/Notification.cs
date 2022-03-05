using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using Newtonsoft.Json;
using ProjectManagementSystem.Models.JobElements;
using ProjectManagementSystem.Models.ProjectElements;
using ProjectManagementSystem.Models.RelationTables;

namespace ProjectManagementSystem.Models.UserElements
{
    public class Notification
    {
        public int Id { get; set; }
        public string action_type { get; set; }
        public string target_type { get; set; }
        public string description { get; set; }

        [Required]
        public string owner_user_id { get; set; }
        [JsonIgnore]
        public User owner_user { get; set; }


        [JsonIgnore]
        public User sender_user { get; set; }
        public string sender_user_id { get; set; }

        [JsonIgnore]
        public Project project { get; set; }
        public int project_id { get; set; }

        [JsonIgnore]
        public Job job { get; set; }
        public int job_id { get; set; }

    }
}

using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using ProjectManagementSystem.Models.ProjectElements;
using ProjectManagementSystem.Models.RelationTables;
using ProjectManagementSystem.Models.UserElements;

namespace ProjectManagementSystem.Models.JobElements
{
    public class Job
    {
        [Key]
        public int Id { get; set; }

        public int section_id { get; set; }
        
        public Section section { get; set; }

        [Required]
        public string jobTitle { get; set; }
        public string jobDescription { get; set; }
        public DateTime startDate { get; set; } = DateTime.Now;
        public DateTime endDate { get; set; }
        public bool isFinished { get; set; } = false;
        public int order_no { get; set; }
        [Required]
        public string createUserId { get; set; }
        public User createUser { get; set; }

        public string receiverUserId { get; set; } 
        public User receiverUser { get; set; }

        public int project_id { get; set; }
        public Project project { get; set; }

        public List<ActivityHistory> activityHistories { get; set; }
        public List<CheckList> checkLists { get; set; }
        public List<Attachment> attachments { get; set; }
        public List<Tags> tags { get; set; }
        public List<JobHasUsers> jobHasUsers { get; set; }
    }
}

using System;
using System.ComponentModel.DataAnnotations;
using Newtonsoft.Json;

namespace ProjectManagementSystem.Models.JobElements
{
    public class ActivityHistory
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public int job_id { get; set; }

        [JsonIgnore]
        public Job job { get; set; }

        public string activityType { get; set; }
        public string detail { get; set; }
        public DateTime date { get; set; } = DateTime.Now;
    }
}

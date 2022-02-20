using System;
using System.ComponentModel.DataAnnotations;

namespace ProjectManagementSystem.Models.JobElements
{
    public class ActivityHistory
    {
        [Key]
        public int Id { get; set; }
        [Required]
        public int job_id { get; set; }
        public Job job { get; set; }
        public DateTime date { get; set; } = DateTime.Now;
    }
}

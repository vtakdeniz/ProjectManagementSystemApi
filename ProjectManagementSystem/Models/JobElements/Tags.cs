using System;
using System.ComponentModel.DataAnnotations;
using Newtonsoft.Json;
using ProjectManagementSystem.Models.JobElements;

namespace ProjectManagementSystem.Models.JobElements
{
    public class Tags
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public int job_id { get; set; }
        [JsonIgnore]
        public Job job { get; set; }

        [Required]
        public string tagName { get; set; }
    }
}

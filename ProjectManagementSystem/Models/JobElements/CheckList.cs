using System;
using System.ComponentModel.DataAnnotations;
using Newtonsoft.Json;

namespace ProjectManagementSystem.Models.JobElements
{
    public class CheckList
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public int job_id { get; set; }

        [JsonIgnore]
        public Job job { get; set; }

        [Required]
        public string text { get; set; }
        public bool isSelected { get; set; }
    }
}

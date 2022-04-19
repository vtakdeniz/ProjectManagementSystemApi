using System;
using System.ComponentModel.DataAnnotations;
using Newtonsoft.Json;

namespace ProjectManagementSystem.Models.JobElements
{
    public class Attachment
    {
        [Key]
        public int Id { get; set; }
        public int job_id { get; set; }
        [JsonIgnore]
        public Job job { get; set; }
        [MaxLength(100)]
        public string name { get; set; }
        [MaxLength(100)]
        public string fileType { get; set; }
        [MaxLength]
        public byte[] fileData { get; set; }
        public DateTime? createdOn { get; set; }
    }
}

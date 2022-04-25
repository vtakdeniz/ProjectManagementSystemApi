using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using ProjectManagementSystem.Models.JobElements;
using Newtonsoft.Json;

namespace ProjectManagementSystem.Models.ProjectElements
{
    public class Section
    {
        [Key]
        public int Id { get; set; }
        public string sectionName { get; set; }

        public int board_id { get; set; }

        [JsonIgnore]
        public Board board { get; set; }

        [JsonIgnore]
        public int job_id { get; set; }
        public Job job { get; set; }
    }
}

using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using ProjectManagementSystem.Models.JobElements;

namespace ProjectManagementSystem.Models.ProjectElements
{
    public class Section
    {
        [Key]
        public int Id { get; set; }
        public string sectionName { get; set; }

        public int board_id { get; set; }
        public Board board { get; set; }

        public List<Job> jobs { get; set; }
    }
}

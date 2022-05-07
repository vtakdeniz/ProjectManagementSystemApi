using System;
using System.Collections.Generic;

namespace ProjectManagementSystem.Dto.JobDto
{
    public class ReadSectionDto
    {
        public int Id { get; set; }
        public string sectionName { get; set; }
        public int board_id { get; set; }
        public List<ReadJobDto> jobs { get; set; }
    }
}

using System;
using System.Collections.Generic;
using ProjectManagementSystem.Dto.UserDto;
using ProjectManagementSystem.Models.JobElements;

namespace ProjectManagementSystem.Dto.JobDto
{
    public class ReadJobDto
    {
        public int Id { get; set; }

        public int section_id { get; set; }
        public string jobTitle { get; set; }
        public string jobDescription { get; set; }
        public DateTime startDate { get; set; } = DateTime.Now;
        public DateTime endDate { get; set; }
        public bool isFinished { get; set; } = false;
        public string createUserId { get; set; }
        public string receiverUserId { get; set; }
        public int project_id { get; set; }
        public int order_no { get; set; }

        public List<ActivityHistory> activityHistories { get; set; }
        public List<CheckList> checkLists { get; set; }
        public List<Attachment> attachments { get; set; }
        public List<Tags> tags { get; set; }
        public List<ReadUserDto> jobHasUsers { get; set; }

    }
}

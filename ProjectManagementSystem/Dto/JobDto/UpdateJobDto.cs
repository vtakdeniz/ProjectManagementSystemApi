using System;
namespace ProjectManagementSystem.Dto.JobDto
{
    public class UpdateJobDto
    {
        public int section_id { get; set; }
        public string jobTitle { get; set; }
        public string jobDescription { get; set; }
        public DateTime startDate { get; set; } 
        public DateTime endDate { get; set; }
        public string receiverUserId { get; set; }
    }
}

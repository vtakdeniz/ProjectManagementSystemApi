using System;
namespace ProjectManagementSystem.Dto.ProjectDto
{
    public class UpdateBoardDto
    {
        public string board_name { get; set; }
        public string description { get; set; }
        public DateTime startDate { get; set; } = DateTime.Now;
        public DateTime endDate { get; set; } = DateTime.MaxValue;
    }
}

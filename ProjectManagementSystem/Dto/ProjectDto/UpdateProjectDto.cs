using System;
namespace ProjectManagementSystem.Dto.ProjectDto
{
    public class UpdateProjectDto
    {
        public string projectName { get; set; }
        public string projectDescription { get; set; }
        public DateTime startDate { get; set; } = DateTime.Now;
        public DateTime endDate { get; set; } = DateTime.MaxValue;
        public bool isFinished { get; set; } = false;
    }
}

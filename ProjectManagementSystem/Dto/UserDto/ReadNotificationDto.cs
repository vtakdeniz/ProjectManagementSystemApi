using System;
using System.ComponentModel.DataAnnotations;
using Newtonsoft.Json;
using ProjectManagementSystem.Dto.BoardReadDto;
using ProjectManagementSystem.Dto.JobDto;
using ProjectManagementSystem.Dto.ProjectDto;
using ProjectManagementSystem.Models.JobElements;
using ProjectManagementSystem.Models.ProjectElements;
using ProjectManagementSystem.Models.UserElements;

namespace ProjectManagementSystem.Dto.UserDto
{
    public class ReadNotificationDto
    {
        public int Id { get; set; }
        public string action_type { get; set; }
        public string target_type { get; set; }
        public string description { get; set; }
        public string owner_user_id { get; set; }
        public ReadUserDto owner_user { get; set; }
        public ReadUserDto sender_user { get; set; }
        public string sender_user_id { get; set; }
        public ReadProjectDto project { get; set; }
        public int project_id { get; set; }
        public ReadBoardDto board { get; set; }
        public int board_id { get; set; }
        public ReadJobDto job { get; set; }
        public int job_id { get; set; }
    }
}

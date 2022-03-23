using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using AutoMapper;
using ProjectManagementSystem.Dto.ProjectDto;
using ProjectManagementSystem.Dto.UserDto;
using ProjectManagementSystem.Models.ProjectElements;
using ProjectManagementSystem.Models.RelationTables;
using ProjectManagementSystem.Models.UserElements;

namespace ProjectManagementSystem.Dto.ProjectDto
{
    public class CreateBoardDto
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public int project_id { get; set; }

        [Required]
        public string board_name { get; set; }

        public string description { get; set; }
        public DateTime startDate { get; set; } = DateTime.Now;
        public DateTime endDate { get; set; } = DateTime.MaxValue;
        public bool isFinished { get; set; } = false;
        public List<int> team_ids { get; set; }
        public List<string> user_ids { get; set; }
    }
}

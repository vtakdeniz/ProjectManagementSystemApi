using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using AutoMapper;
using ProjectManagementSystem.Dto.ProjectDto;
using ProjectManagementSystem.Dto.UserDto;
using ProjectManagementSystem.Models.ProjectElements;
using ProjectManagementSystem.Models.RelationTables;
using ProjectManagementSystem.Models.UserElements;

namespace ProjectManagementSystem.Dto.ReadBoardDto
{
    public class ReadBoardDto
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

        public List<Section> sections { get; set; }
        public List<ReadUserDto> boardHasAdmins { get; set; }
        public List<ReadTeamDto> boardHasTeams { get; set; }
        public List<ReadUserDto> boardHasUsers { get; set; }
    }
}

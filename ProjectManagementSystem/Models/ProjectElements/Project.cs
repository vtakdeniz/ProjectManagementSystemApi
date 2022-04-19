using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using AutoMapper;
using ProjectManagementSystem.Models.JobElements;
using ProjectManagementSystem.Models.RelationTables;

namespace ProjectManagementSystem.Models.ProjectElements
{
    public class Project
    {
        [Key]
        public int Id { get; set; }
        [Required]
        public string projectName { get; set; }
        public string projectDescription { get; set; }
        [Required]
        public DateTime startDate { get; set; } = DateTime.Now;
        public DateTime endDate { get; set; } = DateTime.MaxValue;
        public bool isFinished { get; set; } = false;
        public List<Team> teams { get; set; }
        public List<Board> boards { get; set; }
        public List<UserAssignedProjects> userAssignedProjects { get; set; }
        public List<UserHasProjects> userHasProjects{ get; set; }
        public List<Job> projectJobs { get; set; }
    }
}

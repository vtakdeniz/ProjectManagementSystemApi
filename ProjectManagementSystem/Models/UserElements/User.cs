using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using ProjectManagementSystem.Models.JobElements;
using ProjectManagementSystem.Models.RelationTables;
using Microsoft.AspNetCore.Identity;

namespace ProjectManagementSystem.Models.UserElements
{
    public class User : IdentityUser
    {
       /* [Required]
        public string userName { get; set; }
        [EmailAddress]
        [Required]
        public string email { get; set; }*/
        [Required]
        public string lastName { get; set; }
        [Required]
        public string firstName { get; set; }

        /*[Required]
        public string password { get; set; }*/

        //public Passwords password { get; set; }

        public List<BoardHasAdmins> boardHasAdmins { get; set; }
        public List<BoardHasUsers> boardHasUsers { get; set; }
        public List<JobHasUsers> jobHasUsers{ get; set; }
        public List<TeamHasUsers> teamHasUsers{ get; set; }
        public List<UserAssignedProjects> userAssignedProjects{ get; set; }
        public List<UserAssignedProjects> assignedByUser { get; set; }
        public List<UserHasProjects> userHasProjects { get; set; }

        public List<Job> userCreatedJobs { get; set; }
        public List<Job> userReceivedJobs { get; set; }


    }
}

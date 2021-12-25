using System;
using ProjectManagementSystem.Models.UserElements;
using ProjectManagementSystem.Models.TaskElements;
using System.ComponentModel.DataAnnotations;

namespace ProjectManagementSystem.Models.RelationTables
{
    public class TaskHasUser
    {
        [Key]
        public User user { get; set; }
        [Key]
        public Task task { get; set; }
    }
}

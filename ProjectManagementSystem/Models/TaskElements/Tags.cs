using System;
using System.ComponentModel.DataAnnotations;
using ProjectManagementSystem.Models.TaskElements;

namespace ProjectManagementSystem.Models.TaskElements
{
    public class Tags
    {
        [Key]
        public int Id { get; set; }
        [Required]
        public Task task { get; set; }
        [Required]
        public string tagName { get; set; }
    }
}

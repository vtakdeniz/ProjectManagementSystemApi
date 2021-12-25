using System;
using System.ComponentModel.DataAnnotations;

namespace ProjectManagementSystem.Models.TaskElements
{
    public class ActivityHistory
    {
        [Key]
        public int Id { get; set; }
        [Required]
        public Task task { get; set; }
        public DateTime date { get; set; } = DateTime.Now;
    }
}

using System;
using System.ComponentModel.DataAnnotations;

namespace ProjectManagementSystem.Models.TaskElements
{
    public class Attachment
    {
        [Key]
        public int Id { get; set; }
        [Required]
        public Task task { get; set; }
    }
}

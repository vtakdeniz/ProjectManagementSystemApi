using System;
using System.ComponentModel.DataAnnotations;

namespace ProjectManagementSystem.Models.TaskElements
{
    public class CheckList
    {
        [Key]
        public int Id { get; set; }
        [Required]
        public Task task { get; set; }
        [Required]
        public string text { get; set; }
        public bool isSelected { get; set; }
    }
}

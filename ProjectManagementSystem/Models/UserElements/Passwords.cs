using System;
using System.ComponentModel.DataAnnotations;

namespace ProjectManagementSystem.Models.UserElements
{
    public class Passwords
    {
        [Key]
        public int userId { get; set; }
        [Required]
        public User user { get; set; }
    }
}

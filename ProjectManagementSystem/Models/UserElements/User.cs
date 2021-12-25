using System;
using System.ComponentModel.DataAnnotations;

namespace ProjectManagementSystem.Models.UserElements
{
    public class User
    {
        [Key]
        public int Id { get; set; }
        [Required]
        public string userName { get; set; }
        [Required]
        public string password { get; set; }
        [EmailAddress]
        [Required]
        public string email { get; set; }
        [Required]
        public string lastName { get; set; }
        [Required]
        public string firstName { get; set; }
    }
}

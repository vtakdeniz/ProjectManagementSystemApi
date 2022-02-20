using System;
using System.ComponentModel.DataAnnotations;

namespace ProjectManagementSystem.Dto.UserDto
{
    public class RegisterUserDto
    {
        [Required]
        public string userName { get; set; }
        [EmailAddress]
        [Required]
        public string email { get; set; }
        [Required]
        public string lastName { get; set; }
        [Required]
        public string firstName { get; set; }
        [Required]
        public string password { get; set; }
    }
}

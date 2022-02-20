using System;
using System.ComponentModel.DataAnnotations;

namespace ProjectManagementSystem.Dto.UserDto
{
    public class LoginUserDto
    {
        [Required]
        public string userName { get; set; }
        [Required]
        public string password { get; set; }
    }
}

using System;
using System.ComponentModel.DataAnnotations;

namespace ProjectManagementSystem.Dto.UserDto
{
    public class UpdateUserDto
    {
        [EmailAddress]
        public string email { get; set; }
        public string lastName { get; set; }
        public string firstName { get; set; }
    }
}

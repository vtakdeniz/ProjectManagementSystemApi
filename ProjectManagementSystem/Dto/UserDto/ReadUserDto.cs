using System;
using System.ComponentModel.DataAnnotations;

namespace ProjectManagementSystem.Dto.UserDto
{
    public class ReadUserDto
    {
        [Key]
        public string Id { get; set; }
        [Required]
        public string userName { get; set; }
        [EmailAddress]
        [Required]
        public string email { get; set; }
        [Required]
        public string lastName { get; set; }
        [Required]
        public string firstName { get; set; }

    }
}

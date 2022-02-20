using System;
using System.ComponentModel.DataAnnotations;

namespace ProjectManagementSystem.Models.UserElements
{
    public class Passwords
    {
        public User user { get; set; }
        [Key]
        public int user_id { get; set; }
        public string hashed_password { get; set; }
      
    }
}

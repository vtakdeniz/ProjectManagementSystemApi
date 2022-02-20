 using System;
using ProjectManagementSystem.Models.UserElements;
using ProjectManagementSystem.Models.ProjectElements;
using System.ComponentModel.DataAnnotations;

namespace ProjectManagementSystem.Models.RelationTables
{
    public class BoardHasAdmins
    {
        public int board_id { get; set; }
        public Board board { get; set; }

        public string user_id { get; set; }
        public User user { get; set; }
    }
}

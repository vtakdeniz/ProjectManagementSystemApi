 using System;
using ProjectManagementSystem.Models.UserElements;
using ProjectManagementSystem.Models.ProjectElements;
using System.ComponentModel.DataAnnotations;
using Newtonsoft.Json;
using AutoMapper;

namespace ProjectManagementSystem.Models.RelationTables
{
    public class BoardHasAdmins
    {
        [JsonIgnore]
        [IgnoreMap]
        public int board_id { get; set; }
        [JsonIgnore]
        [IgnoreMap]
        public Board board { get; set; }
        [JsonIgnore]
        [IgnoreMap]
        public string user_id { get; set; }
        public User user { get; set; }
    }
}

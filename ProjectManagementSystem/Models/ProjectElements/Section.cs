using System;
using System.ComponentModel.DataAnnotations;

namespace ProjectManagementSystem.Models.ProjectElements
{
    public class Section
    {
        [Key]
        public int Id { get; set; }
        public string sectionName { get; set; }
        public Board board { get; set; }
    }
}

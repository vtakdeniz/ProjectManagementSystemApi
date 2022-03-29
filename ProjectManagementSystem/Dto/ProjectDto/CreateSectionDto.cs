using System;
using System.ComponentModel.DataAnnotations;

namespace ProjectManagementSystem.Dto.ProjectDto
{
    public class CreateSectionDto
    {
        [Key]
        public int Id { get; set; }
        public string sectionName { get; set; }

        public int board_id { get; set; }
    }
}

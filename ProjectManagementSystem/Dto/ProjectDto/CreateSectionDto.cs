﻿using System;
using System.ComponentModel.DataAnnotations;

namespace ProjectManagementSystem.Dto.ProjectDto
{
    public class CreateSectionDto
    {
        public string sectionName { get; set; }
        public int board_id { get; set; }
    }
}

using System;
using AutoMapper;
using System.Collections.Generic;
using System.Linq;
using ProjectManagementSystem.Models.UserElements;
using ProjectManagementSystem.Dto.UserDto;
using ProjectManagementSystem.Dto.ProjectDto;
using ProjectManagementSystem.Models.ProjectElements;
using ProjectManagementSystem.Models.RelationTables;

namespace ProjectManagementSystem.MappingProfile
{
    public class ManagementMapping : Profile
    {
        public ManagementMapping()
        {

            CreateMap<User, ReadUserDto>();
            CreateMap<RegisterUserDto, User>();


            CreateMap<Project, ReadProjectDto>();
            CreateMap<CreateProjectDto, Project>().ForMember(dto=>dto.isFinished,opt=>opt.Ignore());

        }
    }
}

using System;
using AutoMapper;
using System.Collections.Generic;
using System.Linq;
using ProjectManagementSystem.Models.UserElements;
using ProjectManagementSystem.Dto.UserDto;
using ProjectManagementSystem.Dto.ProjectDto;
using ProjectManagementSystem.Models.ProjectElements;
using ProjectManagementSystem.Models.RelationTables;
using ProjectManagementSystem.Dto.ReadBoardDto;

namespace ProjectManagementSystem.MappingProfile
{
    public class ManagementMapping : Profile
    {
        public ManagementMapping()
        {

            CreateMap<User, ReadUserDto>();
            CreateMap<RegisterUserDto, User>();

            CreateMap<Project, ReadProjectDto>();
            CreateMap<CreateProjectDto, Project>()
                .ForMember(dto=>dto.isFinished,opt=>opt.Ignore());

            CreateMap<CreateBoardDto, Board>();
            CreateMap<Board, ReadBoardDto>()
                .ForMember(dest => dest.boardHasAdmins,
                    src => src.MapFrom(src => src.boardHasAdmins.Select(s => s.user)))
                .ForMember(dest => dest.boardHasUsers,
                    src => src.MapFrom(src => src.boardHasUsers.Select(s => s.user)));

            CreateMap<Team, ReadTeamDto>();
            CreateMap<CreateTeamDto, Team>();

            CreateMap<CreateSectionDto, Section>();

        }
    }
}

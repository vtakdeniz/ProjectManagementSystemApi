using System;
using AutoMapper;
using System.Collections.Generic;
using System.Linq;
using ProjectManagementSystem.Models.UserElements;
using ProjectManagementSystem.Dto.UserDto;
using ProjectManagementSystem.Dto.ProjectDto;
using ProjectManagementSystem.Models.ProjectElements;
using ProjectManagementSystem.Models.RelationTables;
using ProjectManagementSystem.Dto.BoardReadDto;
using ProjectManagementSystem.Models.JobElements;
using ProjectManagementSystem.Dto.JobDto;

namespace ProjectManagementSystem.MappingProfile
{
    public class ManagementMapping : Profile
    {
        public ManagementMapping()
        {

            CreateMap<User, ReadUserDto>();
            CreateMap<RegisterUserDto, User>();

            CreateMap<Project, ReadProjectDto>()
                .ForMember(dest => dest.userAssignedProjects,
                    src => src.MapFrom(src => src.userAssignedProjects.Select(s => s.receiverUser)))
            .ForMember(dest => dest.userHasProjects,
                    src => src.MapFrom(src => src.userHasProjects.Select(s => s.user)));
            CreateMap<Project, UpdateProjectDto>();
            CreateMap<UpdateProjectDto,Project>()
                .ForMember(dto => dto.userAssignedProjects, opt => opt.Ignore())
                .ForMember(dto => dto.userHasProjects, opt => opt.Ignore())
                .ForMember(dto => dto.teams, opt => opt.Ignore())
                .ForMember(dto => dto.boards, opt => opt.Ignore())
                .ForMember(dto => dto.projectJobs, opt => opt.Ignore());
            CreateMap<CreateProjectDto, Project>()
                .ForMember(dto=>dto.isFinished,opt=>opt.Ignore());

            CreateMap<Board, UpdateBoardDto>();
            CreateMap<UpdateBoardDto, Board>()
                .ForMember(dto => dto.sections, opt => opt.Ignore())
                .ForMember(dto => dto.boardHasAdmins, opt => opt.Ignore())
                .ForMember(dto => dto.boardHasTeams, opt => opt.Ignore())
                .ForMember(dto => dto.boardHasUsers, opt => opt.Ignore())
                .ForMember(dto => dto.project_id, opt => opt.Ignore());

            CreateMap<CreateBoardDto, Board>();
            CreateMap<Board, ReadBoardDto>()
                .ForMember(dest => dest.boardHasAdmins,
                    src => src.MapFrom(src => src.boardHasAdmins.Select(s => s.user)))
                .ForMember(dest => dest.boardHasUsers,
                    src => src.MapFrom(src => src.boardHasUsers.Select(s => s.user)))
                .ForMember(dest => dest.boardHasTeams,
                    src => src.MapFrom(src => src.boardHasTeams.Select(s => s.team)));

            CreateMap<Team, ReadTeamDto>()
                .ForMember(dest => dest.users,
                    src => src.MapFrom(src => src.teamHasUsers.Select(s => s.user)));
            CreateMap<CreateTeamDto, Team>();

            CreateMap<CreateSectionDto, Section>()
                .ForMember(dto => dto.order_no, opt => opt.Ignore());
            CreateMap<Section,ReadSectionDto>();

            CreateMap<Job, ReadJobDto>()
                 .ForMember(dest => dest.jobHasUsers,
                    src => src.MapFrom(src => src.jobHasUsers.Select(s => s.user)));
            CreateMap<CreateJobDto, Job>()
                .ForMember(dto => dto.isFinished, opt => opt.Ignore())
                .ForMember(dto => dto.Id, opt => opt.Ignore())
                .ForMember(dto => dto.jobHasUsers, opt => opt.Ignore())
                .ForMember(dto => dto.createUserId, opt => opt.Ignore())
                .ForMember(dto => dto.tags, opt => opt.Ignore())
                .ForMember(dto => dto.order_no, opt => opt.Ignore());
            CreateMap<Job, UpdateJobDto>();
            CreateMap<UpdateJobDto,Job >()
                .ForMember(dto => dto.activityHistories, opt => opt.Ignore())
                .ForMember(dto => dto.checkLists, opt => opt.Ignore())
                .ForMember(dto => dto.attachments, opt => opt.Ignore())
                .ForMember(dto => dto.tags, opt => opt.Ignore())
                .ForMember(dto => dto.jobHasUsers, opt => opt.Ignore());

            CreateMap<CreateTagDto, Tags>();
            
            CreateMap<CreateChecklistDto, CheckList>();

            CreateMap<Notification, ReadNotificationDto>();

        }
    }
}

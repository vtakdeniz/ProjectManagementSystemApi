using System;
using Microsoft.EntityFrameworkCore;
using ProjectManagementSystem.Models.ProjectElements;
using ProjectManagementSystem.Models.UserElements;
using ProjectManagementSystem.Models.RelationTables;
using ProjectManagementSystem.Models.JobElements;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;

namespace ProjectManagementSystem.Data
{
    public class ManagementContext :  IdentityDbContext<User>
    {
        public ManagementContext(DbContextOptions<ManagementContext> opt):base(opt)
        {
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {

            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<Notification>()
                .HasOne(t => t.owner_user)
                .WithMany(p => p.notifications)
                .HasForeignKey(t => t.owner_user_id);

            modelBuilder.Entity<Team>()
                .HasOne(t => t.project)
                .WithMany(p => p.teams)
                .HasForeignKey(t=>t.project_id);

            modelBuilder.Entity<Section>()
                .HasOne(t => t.board)
                .WithMany(p => p.sections)
                .HasForeignKey(s=>s.board_id).OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<Section>()
             .HasMany(s => s.jobs)
             .WithOne(s => s.section).OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<Job>()
                .HasOne(j => j.createUser)
                .WithMany(u=>u.userCreatedJobs)
                .HasForeignKey(j=>j.createUserId);

            modelBuilder.Entity<Job>()
                .HasOne(j => j.receiverUser)
                .WithMany(u => u.userReceivedJobs).OnDelete(DeleteBehavior.NoAction)
                .HasForeignKey(j => j.receiverUserId).OnDelete(DeleteBehavior.NoAction);

            modelBuilder.Entity<Job>()
                .HasMany(j=>j.activityHistories)
                .WithOne(a=>a.job)
                .HasForeignKey(a => a.job_id);

            modelBuilder.Entity<Job>()
                .HasMany(j => j.attachments)
                .WithOne(a => a.job)
                .HasForeignKey(a => a.job_id);

            modelBuilder.Entity<Job>()
                .HasMany(j => j.checkLists)
                .WithOne(a => a.job)
                .HasForeignKey(a => a.job_id);

            modelBuilder.Entity<Job>()
                .HasMany(j => j.tags)
                .WithOne(a => a.job)
                .HasForeignKey(a => a.job_id);

            modelBuilder.Entity<Project>()
               .HasMany(j => j.projectJobs)
               .WithOne(a => a.project).OnDelete(DeleteBehavior.NoAction);

            modelBuilder.Entity<Project>()
               .HasMany(p => p.boards)
               .WithOne(b => b.project).OnDelete(DeleteBehavior.Cascade);

            ManyToManyRelationshipConfiguration(modelBuilder);

        }

        private void ManyToManyRelationshipConfiguration(ModelBuilder modelBuilder) {

            modelBuilder.Entity<BoardHasAdmins>()
                .HasKey(r=>new { r.board_id,r.user_id });
            modelBuilder.Entity<BoardHasAdmins>()
                .HasOne(r => r.board)
                .WithMany(board => board.boardHasAdmins)
                .HasForeignKey(r=>r.board_id).OnDelete(DeleteBehavior.Cascade);
            modelBuilder.Entity<BoardHasAdmins>()
                .HasOne(r => r.user)
                .WithMany(user => user.boardHasAdmins)
                .HasForeignKey(r => r.user_id).OnDelete(DeleteBehavior.Cascade);


            modelBuilder.Entity<BoardHasTeams>()
                .HasKey(r => new { r.board_id, r.team_id});
            modelBuilder.Entity<BoardHasTeams>()
                .HasOne(r => r.board)
                .WithMany(board => board.boardHasTeams)
                .HasForeignKey(r => r.board_id).OnDelete(DeleteBehavior.Restrict);
            modelBuilder.Entity<BoardHasTeams>()
                .HasOne(r => r.team)
                .WithMany(team => team.boardHasTeams)
                .HasForeignKey(r => r.team_id).OnDelete(DeleteBehavior.Cascade);

            
            modelBuilder.Entity<BoardHasUsers>()
                .HasKey(r => new { r.user_id, r.board_id});
            modelBuilder.Entity<BoardHasUsers>()
                .HasOne(r => r.board)
                .WithMany(board => board.boardHasUsers)
                .HasForeignKey(r => r.board_id).OnDelete(DeleteBehavior.Cascade);
            modelBuilder.Entity<BoardHasUsers>()
                .HasOne(r => r.user)
                .WithMany(user => user.boardHasUsers)
                .HasForeignKey(r => r.user_id).OnDelete(DeleteBehavior.Cascade);


            modelBuilder.Entity<JobHasUsers>()
                .HasKey(r => new { r.user_id, r.job_id});
            modelBuilder.Entity<JobHasUsers>()
                .HasOne(r => r.job)
                .WithMany(job => job.jobHasUsers)
                .HasForeignKey(r => r.job_id).OnDelete(DeleteBehavior.NoAction);
            modelBuilder.Entity<JobHasUsers>()
                .HasOne(r => r.user)
                .WithMany(user => user.jobHasUsers)
                .HasForeignKey(r => r.user_id).OnDelete(DeleteBehavior.NoAction);


            modelBuilder.Entity<TeamHasUsers>()
              .HasKey(r => new { r.team_id, r.user_id});
            modelBuilder.Entity<TeamHasUsers>()
                .HasOne(r => r.team)
                .WithMany(team => team.teamHasUsers)
                .HasForeignKey(r => r.team_id).OnDelete(DeleteBehavior.NoAction);
            modelBuilder.Entity<TeamHasUsers>()
                .HasOne(r => r.user)
                .WithMany(user => user.teamHasUsers)
                .HasForeignKey(r => r.user_id).OnDelete(DeleteBehavior.NoAction);


            modelBuilder.Entity<UserAssignedProjects>()
               .HasOne(r => r.assignerUser)
               .WithMany(user => user.assignedByUser)
               .HasForeignKey(r => r.assigner_id).OnDelete(DeleteBehavior.NoAction)
               .IsRequired();
            modelBuilder.Entity<UserAssignedProjects>()
              .HasKey(r => new { r.receiver_id, r.project_id});
            modelBuilder.Entity<UserAssignedProjects>()
                .HasOne(r => r.project)
                .WithMany(project => project.userAssignedProjects)
                .HasForeignKey(r => r.project_id).OnDelete(DeleteBehavior.NoAction);
            modelBuilder.Entity<UserAssignedProjects>()
                .HasOne(r => r.receiverUser)
                .WithMany(user => user.userAssignedProjects)
                .HasForeignKey(r => r.receiver_id).OnDelete(DeleteBehavior.NoAction);


            modelBuilder.Entity<UserHasProjects>()
              .HasKey(r => new { r.user_id, r.project_id});
            modelBuilder.Entity<UserHasProjects>()
                .HasOne(r => r.project)
                .WithMany(project => project.userHasProjects)
                .HasForeignKey(r => r.project_id).OnDelete(DeleteBehavior.Cascade);
            modelBuilder.Entity<UserHasProjects>()
                .HasOne(r => r.user)
                .WithMany(user => user.userHasProjects)
                .HasForeignKey(r => r.user_id).OnDelete(DeleteBehavior.Cascade);



        }

        public DbSet<User> users { get; set; }
        public DbSet<BoardHasUsers> boardHasUsers { get; set; }
        public DbSet<UserHasProjects> userHasProjects { get; set; }
        public DbSet<UserAssignedProjects> userAssignedProjects { get; set; }
        public DbSet<TeamHasUsers> teamHasUsers { get; set; }
        public DbSet<JobHasUsers> taskHasUsers { get; set; }
        public DbSet<Notification> notifications { get; set; }

        public DbSet<Project> projects { get; set; }
        public DbSet<Team> teams { get; set; }
        public DbSet<Board> boards { get; set; }
        public DbSet<BoardHasTeams> boardHasTeams { get; set; }
        public DbSet<BoardHasAdmins> boardHasAdmins { get; set; }
        public DbSet<Section> sections { get; set; }

        public DbSet<Job> jobs { get; set; }
        public DbSet<ActivityHistory> activityHistories { get; set; }
        public DbSet<CheckList> checkLists { get; set; }
        public DbSet<Attachment> attachments { get; set; }
        public DbSet<Tags> tags { get; set; }


    }
}

using Core.Models;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DAL
{
    public class ApplicationContext : IdentityDbContext<AppUser>
    {
        public ApplicationContext(DbContextOptions<ApplicationContext> options) : base(options)
        {
            
        }

        //protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        //{
        //}

        //public DbSet<AppUser> AppUsers { get; set; }
        public DbSet<Assignment> Assignments { get; set; }
        public DbSet<AssignmentAnswer> AssignmentAnswers { get; set; }
        public DbSet<AssignmentFile> AssignmentFiles { get; set; }
        public DbSet<Course> Courses { get; set; }
        public DbSet<EducationMaterial> EducationMaterials { get; set; }
        public DbSet<Group> Groups { get; set; }
        public DbSet<UserActivity> UserActivities { get; set; }
        // public DbSet<UserAssignments> UserAssignments { get; set; }
        // public DbSet<UserCourses> UserCourses { get; set; }
        // public DbSet<UserGroups> UserGroups { get; set; }
        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);

            builder.Entity<UserCourses>().HasKey(uc => new { uc.CourseId, uc.AppUserId });

            builder.Entity<UserAssignments>().HasKey(ua => new { ua.AppUserId, ua.AssignmentId });

            builder.Entity<UserGroups>().HasKey(ug => new { ug.AppUserId, ug.GroupId });

            builder.Entity<EducationMaterial>()
                .HasOne<Group>(em => em.Group)
                .WithMany(g => g.EducationMaterials)
                .HasForeignKey(em => em.GroupId)
                .OnDelete(DeleteBehavior.ClientSetNull);

            //builder.Entity<UserAssignments>()
            //    .HasMany<AssignmentAnswer>(ua => ua.AssignmentAnswers)
            //    .WithOne(aa => aa.UserAssignment)
            //    .HasForeignKey(aa => aa.UserAssignmentId)
            //    .OnDelete(DeleteBehavior.ClientSetNull);
        }
    }
}


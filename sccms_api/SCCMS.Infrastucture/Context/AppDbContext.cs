using Microsoft.AspNetCore.Http;

using Microsoft.EntityFrameworkCore;
using SCCMS.Infrastucture.Configuration;
using SCCMS.Infrastucture.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;

namespace SCCMS.Infrastucture.Context
{
    public class AppDbContext :DbContext
    {
        private readonly int _currentUserId;
        private readonly IHttpContextAccessor _httpContextAccessor;
        public AppDbContext(DbContextOptions<AppDbContext> options, IHttpContextAccessor httpContextAccessor) : base(options)
        {
            _httpContextAccessor= httpContextAccessor;
        }
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);
            modelBuilder.ApplyConfigurationsFromAssembly(typeof(SupervisorStudentGroupConfig).Assembly);

        }

        public DbSet<User> Users { get; set; }
        public DbSet<Course> Courses { get; set; }
        public DbSet<Team> Teams { get; set; }
        public DbSet<StudentReport> StudentReports { get; set; }
        public DbSet<Volunteer> Volunteers { get; set; }
        public DbSet<VolunteerCourse> VolunteerCourses { get; set; }
        public DbSet<Room> Rooms { get; set; }
        public DbSet<Feedback> Feedbacks { get; set; }
        public DbSet<StudentGroup> StudentGroups { get; set; }
        public DbSet<Report> Reports { get; set; }
        public DbSet<NightShift> NightShifts { get; set; }
        public DbSet<NightShiftAssignment> NightShiftAssignments { get; set; }
        public DbSet<EmailTemplate> EmailTemplates { get; set; }
        public DbSet<Post> Posts { get; set; }
        public DbSet<Student> Students { get; set; }
        public DbSet<StudentCourse> StudentCourses { get; set; }
        public DbSet<StudentGroupAssignment> StudentGroupAssignments { get; set; }
        public DbSet<SupervisorStudentGroup> SupervisorStudentGroups { get; set; }
        public DbSet<Role> Roles { get; set; }
        public DbSet<StaffFreeTime> StaffFreeTimes { get; set; }
        public DbSet<Notification> Notifications { get; set; }

        public int GetCurrentUserId()
        {
            if (_httpContextAccessor.HttpContext == null)
            {
                return -1;
            }

            var userIdClaim = _httpContextAccessor.HttpContext.User.FindFirst("userId");

            if (userIdClaim != null && int.TryParse(userIdClaim.Value, out int userId))
            {
                return userId;
            }
            return -1;
        }

        public override async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
        {
            //TODO: Get current user id from HttpContext
          //  var currentUserId = GetCurrentUserId();
          var currentUserId = GetCurrentUserId();
            if(currentUserId == -1)
            {
                currentUserId = 1;
            }
            var entries = ChangeTracker.Entries().Where(e => e.Entity is BaseEntity && (
                e.State == EntityState.Added || e.State == EntityState.Modified));

            foreach (var entry in entries)
            {
                var entity = (BaseEntity)entry.Entity;
                entity.DateModified = DateTime.Now;
                entity.UpdatedBy = currentUserId;

                if (entry.State == EntityState.Added)
                {
                    entity.DateCreated = DateTime.Now;
                    entity.CreatedBy = currentUserId;
                }
            }

            return await base.SaveChangesAsync(cancellationToken);
        }
    }
}

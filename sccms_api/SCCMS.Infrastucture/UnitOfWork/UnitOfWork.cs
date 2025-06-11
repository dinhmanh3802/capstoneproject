using Microsoft.EntityFrameworkCore;
using SCCMS.Infrastucture.Context;
using SCCMS.Infrastucture.Entities;
using SCCMS.Infrastucture.Repository.Implements;
using SCCMS.Infrastucture.Repository.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SCCMS.Infrastucture.UnitOfWork
{
    public class UnitOfWork : IUnitOfWork
    {
        private readonly AppDbContext _context;

        public UnitOfWork(AppDbContext context)
        {
            _context = context;
            Role = new RoleRepository(_context);
            Course = new CourseRepository(_context);
            Feedback = new FeedbackRepository(_context);
            NightShiftAssignment = new NightShiftAssignmentRepository(_context);
            NightShift = new NightShiftRepository(_context);
            Post = new PostRepository(_context);
            Report = new ReportRepository(_context);
            Room = new RoomRepository(_context);
            StudentCourse = new StudentCourseRepository(_context);
            StudentGroupAssignment = new StudentGroupAssignmentRepository(_context);
            StudentGroup = new StudentGroupRepository(_context);
            Student = new StudentRepository(_context);
            SupervisorStudentGroup = new SupervisorStudentGroupRepository(_context);
            Team = new TeamRepository(_context);
            User = new UserRepository(_context);
            VolunteerApplication = new VolunteerApplicationRepository(_context);
            Volunteer = new VolunteerRepository(_context);
            VolunteerTeam = new VolunteerTeamRepository(_context);
            EmailTemplate = new EmailTemplateRepository(_context);
			StudentReport = new StudentReportRepository(_context);
            VolunteerCourse = new VolunteerCourseRepository(_context);
            StaffFreeTime= new StaffFreeTimeRepository(_context);
		}

        public IStaffFreeTimeRepository StaffFreeTime {  get; set; }
        public ICourseRepository Course { get; set; }
        public IFeedbackRepository Feedback { get; set; }
        public INightShiftAssignmentRepository NightShiftAssignment { get; set; }
        public INightShiftRepository NightShift { get; set; }
        public IPostRepository Post { get; set; }
        public IReportRepository Report { get; set; }
        public IRoleRepository Role {  get; set; }
        public IRoomRepository Room { get; set; }
        public IStudentCourseRepository StudentCourse { get; set; }
        public IStudentGroupAssignmentRepository StudentGroupAssignment { get; set; }
        public IStudentGroupRepository StudentGroup { get; set; }
        public IStudentRepository Student { get; set; }
        public ISupervisorStudentGroupRepository SupervisorStudentGroup { get; set; }
        public ITeamRepository Team { get; set; }
        public IUserRepository User { get; set; }
        public IVolunteerApplicationRepository VolunteerApplication { get; set; }
        public IVolunteerRepository Volunteer { get; set; }
        public IVolunteerTeamRepository VolunteerTeam { get; set; }
        public IEmailTemplateRepository EmailTemplate { get; set; }
		public IStudentReportRepository StudentReport { get; }
        public IVolunteerCourseRepository VolunteerCourse { get; set; }

		public void Dispose()
        {
            _context.Dispose();
        }

        public async Task<int> SaveChangeAsync()
        {
            return await _context.SaveChangesAsync();
        }
    }
}

    using SCCMS.Infrastucture.Entities;
using SCCMS.Infrastucture.Repository;
using SCCMS.Infrastucture.Repository.Interfaces;
using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using System.Threading.Tasks;

    namespace SCCMS.Infrastucture.UnitOfWork
    {
        public interface IUnitOfWork : IDisposable
        {
            ICourseRepository Course { get; }
            IRoleRepository Role { get; }
            IFeedbackRepository Feedback { get; }
            INightShiftAssignmentRepository NightShiftAssignment { get; }
            INightShiftRepository NightShift { get; }
            IPostRepository Post { get; }
            IReportRepository Report { get; }
            IRoomRepository Room { get; }
            IStudentCourseRepository StudentCourse { get; }
            IStudentGroupAssignmentRepository StudentGroupAssignment { get; }
            IStudentGroupRepository StudentGroup { get; }
            IStudentRepository Student { get; }
            ISupervisorStudentGroupRepository SupervisorStudentGroup { get; }
            ITeamRepository Team { get; }
            IUserRepository User { get; }
            IVolunteerApplicationRepository VolunteerApplication { get; }
            IVolunteerRepository Volunteer { get; }
            IVolunteerTeamRepository VolunteerTeam { get; }
        IStaffFreeTimeRepository StaffFreeTime { get; }
            IEmailTemplateRepository EmailTemplate { get; }
		    IStudentReportRepository StudentReport { get; } // Thêm dòng này

            IVolunteerCourseRepository VolunteerCourse { get; }


        Task<int> SaveChangeAsync();
        }
    }

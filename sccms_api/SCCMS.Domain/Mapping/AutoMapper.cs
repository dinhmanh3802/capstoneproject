using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AutoMapper;
using SCCMS.Domain.DTOs.CourseDtos;
using SCCMS.Domain.DTOs.FeedbackDtos;
using SCCMS.Domain.DTOs.NightShiftDtos;
using SCCMS.Domain.DTOs.PostDtos;
using SCCMS.Domain.DTOs.ReportDtos;
using SCCMS.Domain.DTOs.RoomDtos;
using SCCMS.Domain.DTOs.StudentGroupDtos;
using SCCMS.Domain.DTOs.TeamDtos;
using SCCMS.Domain.DTOs.UserDtos;
using SCCMS.Domain.DTOs.VolunteerDtos;
using SCCMS.Domain.DTOs.VolunteerApplicationDtos;
using SCCMS.Infrastucture.Entities;
using SCCMS.Domain.DTOs.StudentDtos;
using SCCMS.Domain.DTOs.StudentCourseDtos;
using SCCMS.Domain.Services.Implements;
using SCCMS.Domain.DTOs.SupervisorDtos;
using SCCMS.Domain.DTOs.StaffFreeTimeDtos;
using SCCMS.Domain.DTOs.VolunteerCourseDtos;
using Utility;
using SCCMS.Domain.DTOs.StudentReportDtos;

namespace SCCMS.Domain.Mapping
{
    public class AutoMapper : Profile
    {
        public AutoMapper()
        {
            CreateMap<Course, CourseDto>().ReverseMap();
            CreateMap<Course, CourseCreateDto>().ReverseMap();
            CreateMap<CourseUpdateDto, Course>()
                .ForAllMembers(options => options.Condition((src, dest, srcMember) =>
                {
                    // Nếu srcMember là null thì bỏ qua ánh xạ
                    if (srcMember == null) return false;

                    // Nếu srcMember là kiểu giá trị, bỏ qua ánh xạ nếu nó bằng giá trị mặc định của kiểu dữ liệu đó
                    if (srcMember is int intVal) return intVal != default;
                    if (srcMember is DateTime dateTimeVal) return dateTimeVal != default(DateTime);

                    // Với các kiểu khác thì luôn ánh xạ nếu không phải null
                    return true;
                }));

            CreateMap<Feedback, FeedbackDto>().ReverseMap();
            CreateMap<Feedback, FeedbackCreateDto>().ReverseMap();
            CreateMap<Feedback, FeedbackUpdateDto>().ReverseMap();
            CreateMap<Feedback, FeedbackMiniDto>().ReverseMap();

            CreateMap<NightShift, NightShiftDto>().ReverseMap();
            CreateMap<NightShift, NightShiftCreateDto>().ReverseMap();
            CreateMap<NightShift, NightShiftUpdateDto>().ReverseMap();

            CreateMap<NightShiftAssignment, NightShiftAssignmentDto>();
            CreateMap<NightShiftAssignment, MyShiftAssignmentDto>().ReverseMap();
            CreateMap<NightShiftAssignment, NightShiftAssignmentCreateDto>().ReverseMap();


            CreateMap<Post, PostDto>().ReverseMap();
            CreateMap<Post, PostCreateDto>().ReverseMap();
            CreateMap<Post, PostUpdateDto>().ReverseMap();

            CreateMap<Report, ReportCreateDto>().ReverseMap();
            CreateMap<Report, ReportUpdateDto>().ReverseMap();

            CreateMap<Room, RoomDto>().ReverseMap();
            CreateMap<Room, RoomCreateDto>().ReverseMap();
            CreateMap<Room, RoomUpdateDto>().ReverseMap();

            CreateMap<Student, StudentDto>().ReverseMap();
            CreateMap<Student, StudentCreateDto>().ReverseMap();
            CreateMap<Student, StudentUpdateDto>().ReverseMap();
            CreateMap<StudentCreateDto, StudentUpdateDto>();

            CreateMap<StudentGroup, StudentGroupDto>().ReverseMap();
            CreateMap<StudentGroup, StudentGroupCreateDto>().ReverseMap();
            CreateMap<StudentGroup, StudentGroupUpdateDto>().ReverseMap();

            CreateMap<StaffFreeTime, StaffFreeTimeDto>()
                .ForMember(dest => dest.UserName, opt => opt.MapFrom(src => src.User.UserName))
                .ForMember(dest => dest.FullName, opt => opt.MapFrom(src=> src.User.FullName))
                .ForMember(dest => dest.Gender, opt => opt.MapFrom(src=> src.User.Gender)).ReverseMap();
            CreateMap<StaffFreeTime, StaffFreeTimeCreateDto>().ReverseMap();
            CreateMap<StaffFreeTime, StaffFreeTimeUpdateDto>().ReverseMap();


            CreateMap<Team, TeamCreateDto>().ReverseMap();
            CreateMap<Team, TeamUpdateDto>().ReverseMap();
            CreateMap<Team, TeamDto>().ReverseMap();

            CreateMap<User, UserDto>().ReverseMap();
            CreateMap<User, UserCreateDto>().ReverseMap();
            CreateMap<User, UserUpdateDto>().ReverseMap();

            CreateMap<User, SupervisorDto>()
                .ForMember(dest => dest.Group, opt => opt.MapFrom(src => src.SupervisorStudentGroup.FirstOrDefault().StudentGroup)).ReverseMap();


            CreateMap<StudentGroup, StudentGroupDto>();

            CreateMap<Volunteer, VolunteerDto>().ReverseMap();
            CreateMap<Volunteer, VolunteerCreateDto>().ReverseMap();
            CreateMap<Volunteer, VolunteerUpdateDto>().ReverseMap();
            CreateMap<VolunteerCreateDto, VolunteerUpdateDto>().ReverseMap();


            CreateMap<VolunteerCourse, VolunteerCourseDto>().ReverseMap();
            CreateMap<VolunteerCourse, VolunteerCourseCreateDto>().ReverseMap();
            CreateMap<VolunteerCourse, VolunteerCourseUpdateDto>().ReverseMap();

            CreateMap<StudentCourse, StudentCourseDto>().ReverseMap();
            CreateMap<StudentCourse, StudentCourseMiniDto>().ReverseMap();
            CreateMap<StudentCourse, StudentCourseCreateDto>().ReverseMap();
            CreateMap<StudentCourse, StudentCourseUpdateDto>().ReverseMap();
            CreateMap<StudentCourse, StudentCourseUpdateReviewerDto>().ReverseMap();
            CreateMap<StudentGroup, StudentGroupDto>()
                 .ForMember(dest => dest.CourseName, opt => opt.MapFrom(src => src.Course.CourseName))
                 .ForMember(dest => dest.Students, opt => opt.MapFrom(src => src.StudentGroupAssignment.Select(sga => sga.Student)))
                 .ForMember(dest => dest.Supervisors, opt => opt.MapFrom(src => src.SupervisorStudentGroup.Select(ssg => ssg.Supervisor)))
                 .ForMember(dest => dest.Reports, opt => opt.MapFrom(src => src.Report));

            CreateMap<Student, SCCMS.Domain.DTOs.StudentGroupDtos.StudentInforDto>();
            CreateMap<User, SupervisorDto>();
            CreateMap<StudentUpdateDto, Student>()
           .ForMember(dest => dest.StudentGroupAssignment, opt => opt.Ignore());

            CreateMap<StudentGroupUpdateDto, StudentGroup>();

            CreateMap<StudentCourse, StudentCourseDto>()
    .ForMember(dest => dest.Student, opt => opt.MapFrom(src => src.Student))
    .ForMember(dest => dest.Course, opt => opt.MapFrom(src => src.Course));

            CreateMap<Student, SCCMS.Domain.DTOs.StudentCourseDtos.StudentInforDto>()
    .ForMember(dest => dest.StudentGroups, opt => opt.MapFrom(src => src.StudentGroupAssignment.Select(sga => sga.StudentGroup)))
    .ForMember(dest => dest.ParentName, opt => opt.MapFrom(src => src.ParentName))
    .ForMember(dest => dest.EmergencyContact, opt => opt.MapFrom(src => src.EmergencyContact));
            CreateMap<Course, DTOs.StudentCourseDtos.CourseInforDto>();

            CreateMap<StudentGroup, StudentGroupInforDto>();
            CreateMap<User, UserInforDto>();

            CreateMap<Post, PostDto>().ReverseMap();
            CreateMap<Post, PostCreateDto>().ReverseMap();
            CreateMap<Post, PostUpdateDto>().ReverseMap();

            //ánh xạ để get thông tin của volunteer
            CreateMap<Volunteer, VolunteerDto>()
    .ForMember(dest => dest.Teams, opt => opt.MapFrom(src => src.VolunteerTeam.Select(vt => new TeamInfoDto
    {
        TeamId = vt.TeamId,
        TeamName = vt.Team.TeamName
    })))
    .ForMember(dest => dest.Courses, opt => opt.MapFrom(src => src.VolunteerCourse.Select(vc => new CourseInfoDto
    {
        CourseId = vc.CourseId,
        CourseName = vc.Course.CourseName
    })));

            // Mapping cho Volunteer sang VolunteerInforDto, bao gồm VolunteerTeam nhưng không gây vòng lặp
            CreateMap<Volunteer, VolunteerInforDto>()
                .ForMember(dest => dest.Teams, opt => opt.MapFrom(src => src.VolunteerTeam));

            CreateMap<VolunteerTeam, TeamInforDto>()
                .ForMember(dest => dest.Id, opt => opt.MapFrom(src => src.TeamId))
                .ForMember(dest => dest.TeamName, opt => opt.MapFrom(src => src.Team.TeamName));

            // Mapping cho VolunteerCourse sang VolunteerCourseDto
            CreateMap<VolunteerCourse, VolunteerCourseDto>()
                .ForMember(dest => dest.Course, opt => opt.MapFrom(src => src.Course))
                .ForMember(dest => dest.Volunteer, opt => opt.MapFrom(src => src.Volunteer))
                .ForMember(dest => dest.Reviewer, opt => opt.MapFrom(src => src.Reviewer));

            // Mapping cho Course sang CourseInforDto
            CreateMap<Course, DTOs.VolunteerCourseDtos.CourseInforDto>();

            // Mapping cho User sang ReviewerInforDto
            CreateMap<User, ReviewerInforDto>();


            CreateMap<VolunteerCourse, VolunteerDto>()
                .ForMember(dest => dest.Id, opt => opt.MapFrom(src => src.Volunteer.Id))
                .ForMember(dest => dest.FullName, opt => opt.MapFrom(src => src.Volunteer.FullName))
                .ForMember(dest => dest.DateOfBirth, opt => opt.MapFrom(src => src.Volunteer.DateOfBirth))
                .ForMember(dest => dest.Gender, opt => opt.MapFrom(src => src.Volunteer.Gender))
                .ForMember(dest => dest.NationalId, opt => opt.MapFrom(src => src.Volunteer.NationalId))
                .ForMember(dest => dest.Address, opt => opt.MapFrom(src => src.Volunteer.Address))
                .ForMember(dest => dest.Status, opt => opt.MapFrom(src => src.Volunteer.Status))
                .ForMember(dest => dest.Note, opt => opt.MapFrom(src => src.Volunteer.Note))
                .ForMember(dest => dest.Image, opt => opt.MapFrom(src => src.Volunteer.Image))
                .ForMember(dest => dest.Courses, opt => opt.MapFrom(src => new List<CourseInfoDto>
                {
                new CourseInfoDto
                {
                    CourseId = src.CourseId,
                    CourseName = src.Course != null ? src.Course.CourseName : "Unknown Course"
                }
                }))
                .ForMember(dest => dest.Teams, opt => opt.MapFrom(src => src.Volunteer.VolunteerTeam.Select(vt => new TeamInfoDto
                {
                    TeamId = vt.TeamId,
                    TeamName = vt.Team.TeamName
                })));
            //ánh xạ phần team
            // Ánh xạ cho Team sang TeamDto
            CreateMap<Team, TeamDto>()
    .ForMember(dest => dest.CourseName, opt => opt.MapFrom(src => src.Course.CourseName))
    .ForMember(dest => dest.LeaderName, opt => opt.MapFrom(src => src.Leader.FullName))
    .ForMember(dest => dest.Volunteers, opt => opt.MapFrom((src, dest, destMember, context) =>
        src.VolunteerTeam.Select(vt => new VolunteerInforInTeamDto
        {
            Id = vt.Volunteer.Id,
            // Lấy volunteerCode theo courseId hiện tại
            volunteerCode = vt.Volunteer.VolunteerCourse
                .FirstOrDefault(vc => vc.CourseId == src.CourseId)?.VolunteerCode ?? string.Empty,
            FullName = vt.Volunteer.FullName,
            DateOfBirth = vt.Volunteer.DateOfBirth,
            Gender = vt.Volunteer.Gender,
            PhoneNumber = vt.Volunteer.PhoneNumber,
            Status = vt.Volunteer.VolunteerCourse
                .FirstOrDefault(vc => vc.CourseId == src.CourseId)?.Status ?? ProgressStatus.Approved,
        }).ToList()
    ));


            // Ánh xạ các thuộc tính cho Volunteer sang VolunteerInforInTeamDto
            CreateMap<Volunteer, VolunteerInforInTeamDto>()
                .ForMember(dest => dest.Id, opt => opt.MapFrom(src => src.Id))
     .ForMember(dest => dest.volunteerCode, opt => opt.MapFrom(src => src.VolunteerCourse != null && src.VolunteerCourse.Any()
         ? src.VolunteerCourse.FirstOrDefault().VolunteerCode
         : string.Empty))
     .ForMember(dest => dest.FullName, opt => opt.MapFrom(src => src.FullName))
     .ForMember(dest => dest.DateOfBirth, opt => opt.MapFrom(src => src.DateOfBirth))
     .ForMember(dest => dest.Gender, opt => opt.MapFrom(src => src.Gender))
.ForMember(dest => dest.Status, opt => opt.MapFrom(src => src.VolunteerCourse != null && src.VolunteerCourse.Any()
        ? src.VolunteerCourse.FirstOrDefault().Status
        : ProgressStatus.Approved)).
ForMember(dest => dest.PhoneNumber, opt => opt.MapFrom(src => src.PhoneNumber));

            CreateMap<StudentGroup, StudentGroupInforDto>().ReverseMap();
            CreateMap<StudentGroup, GroupInfoDto>()
                .ForMember(dest => dest.GroupId, opt => opt.MapFrom(src => src.Id)).ReverseMap();

            CreateMap<User, UserInforDto>().ReverseMap();

            CreateMap<User, UserInforDto>().ReverseMap();

            CreateMap<User, UserInforDto>().ReverseMap();
            CreateMap<Report, ReportDto>()
            .ForMember(dest => dest.StudentGroup, opt => opt.MapFrom(src => src.StudentGroup))
            .ForMember(dest => dest.SubmittedByUser, opt => opt.MapFrom(src => src.SubmittedByUser))
            .ForMember(dest => dest.StudentReports, opt => opt.MapFrom(src => src.StudentReports))
            .ForMember(dest => dest.Room, opt => opt.MapFrom(src => src.Room))
            .ForMember(dest => dest.NightShift, opt => opt.MapFrom(src => src.NightShift))
            .ForMember(dest => dest.IsEditable, opt => opt.Ignore())
            .ForMember(dest => dest.StudentReports, opt => opt.MapFrom(src => src.StudentReports))
            .ForMember(dest => dest.IsSupervisorAssigned, opt => opt.Ignore())
            .ForMember(dest => dest.IsStaffAssigned, opt => opt.Ignore());

            CreateMap<StudentReport, StudentReportDto>()
            .ForMember(dest => dest.StudentId, opt => opt.MapFrom(src => src.StudentId))
            .ForMember(dest => dest.StudentName, opt => opt.MapFrom(src => src.Student.FullName))
            .ForMember(dest => dest.StudentImage, opt => opt.MapFrom(src => src.Student.Image))
            .ForMember(dest => dest.Status, opt => opt.MapFrom(src => src.Status))
            .ForMember(dest => dest.Comment, opt => opt.MapFrom(src => src.Comment))
            // Không ánh xạ StudentCode ở đây
            ;


        }
    }
}

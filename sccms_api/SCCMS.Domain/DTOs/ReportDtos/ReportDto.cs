using SCCMS.Domain.DTOs.NightShiftDtos;
using SCCMS.Domain.DTOs.RoomDtos;
using SCCMS.Domain.DTOs.StudentDtos;
using SCCMS.Domain.DTOs.StudentReportDtos;
using SCCMS.Domain.DTOs.UserDtos;
using SCCMS.Infrastucture.Entities;
using Utility;

namespace SCCMS.Domain.DTOs.ReportDtos
{
    public class ReportDto
    {
        public int Id { get; set; }
        public int CourseId { get; set; }
        public int? StudentGroupId { get; set; }
        public GroupInfoDto StudentGroup { get; set; }
        public int? RoomId { get; set; }
        public RoomDto Room { get; set; }
        public int? NightShiftId { get; set; }
        public NightShiftDto? NightShift { get; set; }
        public DateTime ReportDate { get; set; }
        public string ReportContent { get; set; }
        public ReportType ReportType { get; set; }
        public DateTime? SubmissionDate { get; set; }
        public int? SubmissionBy { get; set; }
        public UserDto SubmittedByUser { get; set; }
        public ReportStatus Status { get; set; }
        public List<StudentReportDto> StudentReports { get; set; }

        public bool IsEditable { get; set; }
        public bool IsSupervisorAssigned { get; set; }
        public bool IsStaffAssigned { get; set; }
    
    }



}
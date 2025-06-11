using Utility;

namespace SCCMS.Domain.DTOs.StudentReportDtos
{
    public class EditAttendanceRequestDto
    {
        public int Id { get; set; } // ID của StudentReport
        public StudentReportStatus Status { get; set; }
        public string? Comment { get; set; }
    }
}
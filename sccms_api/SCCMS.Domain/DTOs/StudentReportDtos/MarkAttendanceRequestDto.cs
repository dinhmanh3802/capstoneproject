// Domain/DTOs/StudentReportDtos/MarkAttendanceRequestDto.cs
using Utility;

namespace SCCMS.Domain.DTOs.StudentReportDtos
{
    public class MarkAttendanceRequestDto
    {
        public int ReportId { get; set; }
        public int StudentId { get; set; }
        public StudentReportStatus Status { get; set; }
        public string? Comment { get; set; }
    }
}
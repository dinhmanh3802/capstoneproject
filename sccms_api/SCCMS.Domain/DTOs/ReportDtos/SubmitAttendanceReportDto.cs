// SCCMS.Domain.DTOs.ReportDtos.SubmitAttendanceReportDto.cs

using SCCMS.Domain.DTOs.StudentReportDtos;
using System.Collections.Generic;

namespace SCCMS.Domain.DTOs.ReportDtos
{
    public class SubmitAttendanceReportDto
    {
        public int ReportId { get; set; }
        public List<StudentReportDto>? StudentReports { get; set; }
        public string? ReportContent { get; set; } // Thêm trường này
    }
}

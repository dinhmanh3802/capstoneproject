using Utility;

namespace SCCMS.Domain.DTOs.StudentReportDtos
{
    public class StudentReportDto
    {
        public int StudentId { get; set; }
        public string? StudentName { get; set; }
        public string? StudentCode { get; set; }
        public string? StudentImage { get; set; }
        public StudentReportStatus Status { get; set; }
        public string? Comment { get; set; }
    }

}
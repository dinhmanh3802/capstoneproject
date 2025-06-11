using SCCMS.Domain.DTOs.ReportDtos;
using SCCMS.Domain.DTOs.SupervisorDtos;
using Utility;

namespace SCCMS.Domain.DTOs.StudentGroupDtos
{
    public class StudentGroupDto
    {
        public int Id { get; set; }
        public int CourseId { get; set; }
        public string CourseName { get; set; }
        public string GroupName { get; set; }
        public Gender Gender { get; set; }
        public List<StudentInforDto> Students { get; set; }
        public List<SupervisorDto> Supervisors { get; set; }
        public List<ReportDto> Reports { get; set; }
    }
}
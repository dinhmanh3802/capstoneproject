// Domain/DTOs/DashboardDtos/RegistrationPerCourseDto.cs
namespace SCCMS.Domain.DTOs.DashboardDtos
{
    public class RegistrationPerCourseDto
    {
        public int CourseId { get; set; }
        public string CourseName { get; set; }
        public int RegistrationCount { get; set; }
    }
}

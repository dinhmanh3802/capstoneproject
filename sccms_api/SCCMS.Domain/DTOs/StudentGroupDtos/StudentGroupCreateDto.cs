// Domain/DTOs/StudentGroupDtos/StudentGroupCreateDto.cs
using Utility;

namespace SCCMS.Domain.DTOs.StudentGroupDtos
{
    public class StudentGroupCreateDto
    {
        public int CourseId { get; set; }
        public string GroupName { get; set; }
        public Gender Gender { get; set; }
        public List<int>? SupervisorIds { get; set; }
    }
}
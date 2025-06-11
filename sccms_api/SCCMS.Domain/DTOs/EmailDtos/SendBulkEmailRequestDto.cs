using SCCMS.Domain.DTOs.CourseDtos;
using SCCMS.Domain.DTOs.StudentDtos;
using SCCMS.Domain.DTOs.VolunteerDtos;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SCCMS.Domain.DTOs.EmailDtos
{
    public class SendBulkEmailRequestDto
    {
        public IEnumerable<int>? ListStudentId { get; set; }
        public IEnumerable<int>? ListVolunteerId { get; set; }
        public int CourseId { get; set; }
        public string Subject { get; set; }
        public string Message { get; set; }
    }
}

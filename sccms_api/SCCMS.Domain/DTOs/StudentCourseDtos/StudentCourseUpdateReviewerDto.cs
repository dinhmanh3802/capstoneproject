using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SCCMS.Domain.DTOs.StudentCourseDtos
{
    public class StudentCourseUpdateReviewerDto
    {
        public List<int> Ids { get; set; }
        public int? ReviewerId { get; set; }
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Utility;

namespace SCCMS.Domain.DTOs.StudentCourseDtos
{
    public class StudentApplicationDetailDto
    {
        public int courseId { get; set; }
        public int id { get; set; }
        public string note { get; set; }
        public ProgressStatus status { get; set; }
        public int studentGroupId { get; set; }
    }
}

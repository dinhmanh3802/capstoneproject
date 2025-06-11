using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SCCMS.Domain.DTOs.EmailDtos
{
    public class StudentApplicationResultRequest
    {
        public int[] ListStudentApplicationId { get; set; }
        public int CourseId { get; set; }
        public string Subject { get; set; }
        public string Message { get; set; }

    }
}

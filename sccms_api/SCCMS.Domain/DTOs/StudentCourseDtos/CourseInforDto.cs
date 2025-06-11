using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Utility;

namespace SCCMS.Domain.DTOs.StudentCourseDtos
{
    public class CourseInforDto
    {
        public int Id { get; set; }

        [Required]
        [StringLength(100)]
        public string? CourseName { get; set; }


        public CourseStatus Status { get; set; } = CourseStatus.notStarted;

    }
}

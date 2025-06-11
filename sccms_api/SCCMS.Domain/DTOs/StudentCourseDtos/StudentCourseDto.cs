using SCCMS.Domain.DTOs.CourseDtos;
using SCCMS.Domain.DTOs.StudentDtos;
using SCCMS.Infrastucture.Entities;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Utility;

namespace SCCMS.Domain.DTOs.StudentCourseDtos
{
    public class StudentCourseDto
    {
        public int Id { get; set; }

        public int CourseId { get; set; }

        public CourseInforDto Course { get; set; } 

        public int StudentId { get; set; }

        public StudentInforDto Student { get; set; } 
        public string StudentCode { get; set; }

        [DataType(DataType.Date)]
        public DateTime ApplicationDate { get; set; }

        public ProgressStatus Status { get; set; }

        [StringLength(500)]
        public string Note { get; set; }

        public int? ReviewerId { get; set; }
        public UserInforDto Reviewer { get; set; }

        [DataType(DataType.Date)]
        public DateTime? ReviewDate { get; set; }
        public int SameNationId { get; set; }

    }
}

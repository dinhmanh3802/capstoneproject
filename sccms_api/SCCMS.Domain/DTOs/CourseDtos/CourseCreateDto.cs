using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SCCMS.Infrastucture.Entities;

namespace SCCMS.Domain.DTOs.CourseDtos
{
    public class CourseCreateDto
    {
        [Required(ErrorMessage = "Tên khóa học là bắt buộc.")]
        [StringLength(100)]
        public string CourseName { get; set; }

        [DataType(DataType.Date)]
        public DateTime StartDate { get; set; }

        [DataType(DataType.Date)]
        public DateTime EndDate { get; set; }

        [Range(1, 1000, ErrorMessage = "Số lượng học sinh dự kiến phải lớn hơn 0")]
        public int ExpectedStudents { get; set; }

        [StringLength(500)]
        public string? Description { get; set; }

        public DateTime StudentApplicationStartDate { get; set; }
        public DateTime StudentApplicationEndDate { get; set; }

        public DateTime VolunteerApplicationStartDate { get; set; }
        public DateTime VolunteerApplicationEndDate { get; set; }
    }
}

using SCCMS.Domain.DTOs.StudentCourseDtos;
using SCCMS.Infrastucture.Entities;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Utility;

namespace SCCMS.Domain.DTOs.StudentDtos
{
    public class StudentDto
    {
		public int? Id { get; set; }

		[Required]
		[StringLength(50)]
		public string FullName { get; set; }


		[DataType(DataType.Date)]
		public DateTime DateOfBirth { get; set; }

		public Gender Gender { get; set; }
		public string Image { get; set; }

		[StringLength(20)]
		public string NationalId { get; set; }
		public string NationalImageFront { get; set; }
		public string NationalImageBack { get; set; }

		[StringLength(255)]
		public string Address { get; set; }

		[StringLength(50)]
		public string ParentName { get; set; }

		[StringLength(50)]
		public string EmergencyContact { get; set; }
		[Required]
		[EmailAddress]
		[StringLength(100)]
		public string? Email { get; set; }

		[StringLength(50)]
		public string? Conduct { get; set; }

		[StringLength(100)]
		public string? AcademicPerformance { get; set; }

		public ProfileStatus Status { get; set; }

		[StringLength(500)]
		public string Note { get; set; }

		// Thay đổi ở đây
		public List<CourseInfoDto> Courses { get; set; }
		public List<GroupInfoDto> Groups { get; set; }
		public List<StudentCourseDto> StudentCourses { get; set; } // Thuộc tính mới để lưu thông tin StudentCourse
	}
}

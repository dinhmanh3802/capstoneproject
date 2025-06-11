using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;
using Utility;

namespace SCCMS.Infrastucture.Entities
{
	public class Course : BaseEntity
	{
		public int Id { get; set; }

		[Required]
		[StringLength(100)]
		public string? CourseName { get; set; }

		[DataType(DataType.Date)]
		public DateTime StartDate { get; set; }

		[DataType(DataType.Date)]
		public DateTime EndDate { get; set; }

		public int ExpectedStudents { get; set; }

		[StringLength(500)]
		public string? Description { get; set; }

		[StringLength(20)]
		public CourseStatus Status { get; set; } = CourseStatus.notStarted;

		public DateTime StudentApplicationStartDate { get; set; }
		public DateTime StudentApplicationEndDate { get; set; }

		public DateTime VolunteerApplicationStartDate { get; set; }
		public DateTime VolunteerApplicationEndDate { get; set; }
		public DateTime? FreeTimeApplicationStartDate { get; set; }
        public DateTime? FreeTimeApplicationEndDate { get; set; }

        public ICollection<StudentCourse>? StudentCourses { get; set; }

		public ICollection<Feedback>? Feedback { get; set; }

		public ICollection<NightShift>? NightShift { get; set; }

		public ICollection<StudentGroup>? StudentGroup { get; set; }
		public ICollection<Room>? Room { get; set; }
	}
}

using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using Utility;

namespace SCCMS.Infrastucture.Entities
{
	public class Volunteer : BaseEntity
	{
		public int Id { get; set; }

		[Required]
		[StringLength(50)]
		public string FullName { get; set; }

		[DataType(DataType.Date)]
		public DateTime? DateOfBirth { get; set; }

		public Gender Gender { get; set; }

        [StringLength(20)]
		public string NationalId { get; set; }

		public string NationalImageFront { get; set; }
		public string NationalImageBack { get; set; }

		[StringLength(255)]
		public string Address { get; set; }
        public string? PhoneNumber { get; set; }
        public string? Email { get; set; }
        public ProfileStatus Status { get; set; }

		[StringLength(255)]
		public string? Note { get; set; }

		// Thuộc tính mới: Image
		public string Image { get; set; } // Đổi từ IFormFile thành string để lưu URL hình ảnh

        public ICollection<VolunteerTeam>? VolunteerTeam { get; set; }
		public ICollection<VolunteerCourse>? VolunteerCourse { get; set; }
	}
}

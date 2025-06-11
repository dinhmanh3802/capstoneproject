using SCCMS.Domain.DTOs.StudentDtos;
using SCCMS.Domain.DTOs.TeamDtos;
using System;
using System.ComponentModel.DataAnnotations;
using Utility;

namespace SCCMS.Domain.DTOs.VolunteerDtos
{
	public class VolunteerDto
	{
		public int Id { get; set; }
		public string FullName { get; set; }
		public DateTime? DateOfBirth { get; set; }
		public Gender Gender { get; set; }
		public string NationalId { get; set; }
		public string NationalImageFront { get; set; }
		public string NationalImageBack { get; set; }
		public string Address { get; set; }
		public ProfileStatus Status { get; set; }
		public string? Note { get; set; }
		public string Image { get; set; }
        public string? PhoneNumber { get; set; }
        public string? Email { get; set; }

        // C�c ??i nh�m m� t�nh nguy?n vi�n tham gia
        public List<TeamInfoDto> Teams { get; set; }

		// C�c kh�a h?c m� t�nh nguy?n vi�n tham gia
		public List<CourseInfoDto> Courses { get; set; }
	}
}

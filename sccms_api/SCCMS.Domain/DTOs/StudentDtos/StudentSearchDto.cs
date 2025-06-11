using System;
using System.Collections.Generic;
using Utility;

namespace SCCMS.Domain.DTOs.StudentDtos
{
	public class StudentSearchDto
	{
		public string? FullName { get; set; }
		public string? EmergencyContact { get; set; }
		public Gender ? Genders { get; set; }
		public DateTime? DateOfBirthStart { get; set; }
		public DateTime? DateOfBirthEnd { get; set; }
		public ProfileStatus? Status { get; set; }
	}
}

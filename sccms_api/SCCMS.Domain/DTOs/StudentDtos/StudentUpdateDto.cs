using Microsoft.AspNetCore.Http;
using SCCMS.Domain.DTOs.CourseDtos;
using SCCMS.Domain.DTOs.StudentGroupDtos;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Utility;

namespace SCCMS.Domain.DTOs.StudentDtos
{
    public class StudentUpdateDto
    {

		public string FullName { get; set; }
		public DateTime DateOfBirth { get; set; }
		public Gender Gender { get; set; }
		public IFormFile? Image { get; set; }
		public string NationalId { get; set; }
		public IFormFile? NationalImageFront { get; set; }
		public IFormFile? NationalImageBack { get; set; }
		public string Address { get; set; }
		public string ParentName { get; set; }
		public string EmergencyContact { get; set; }
		public string Email { get; set; }
		public string Conduct { get; set; }
		public string AcademicPerformance { get; set; }
		public ProfileStatus Status { get; set; }
		public string? Note { get; set; }
		
	}
}

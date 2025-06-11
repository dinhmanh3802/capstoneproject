using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Utility;

namespace SCCMS.Domain.DTOs.StudentDtos
{
    public class StudentCreateDto
    {
		public string FullName { get; set; }

		[DataType(DataType.Date)]
		public DateTime DateOfBirth { get; set; }

		public Gender Gender { get; set; }
		public IFormFile Image { get; set; }

		[StringLength(20)]
		public string NationalId { get; set; }
		public IFormFile NationalImageFront { get; set; }
		public IFormFile NationalImageBack { get; set; }

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

		public ProfileStatus? Status { get; set; }
	
	}
}

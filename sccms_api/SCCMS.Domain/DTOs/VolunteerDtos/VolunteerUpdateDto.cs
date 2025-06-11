using Microsoft.AspNetCore.Http;
using System;
using System.ComponentModel.DataAnnotations;
using Utility;

namespace SCCMS.Domain.DTOs.VolunteerDtos
{
	public class VolunteerUpdateDto
	{
        
        [StringLength(50)]
        public string FullName { get; set; }

        [DataType(DataType.Date)]
        public DateTime DateOfBirth { get; set; }

        public Gender Gender { get; set; }

        [StringLength(20)]
        public string NationalId { get; set; }

        public IFormFile? NationalImageFront { get; set; }
        public IFormFile? NationalImageBack { get; set; }
        public IFormFile? Image { get; set; }

        [StringLength(255)]
        public string Address { get; set; }

        public ProfileStatus Status { get; set; }

        [StringLength(255)]
        public string? Note { get; set; }

        
        [EmailAddress]
        [StringLength(100)]
        public string Email { get; set; }
        [StringLength(10)]
        public string PhoneNumber { get; set; }
    }
}

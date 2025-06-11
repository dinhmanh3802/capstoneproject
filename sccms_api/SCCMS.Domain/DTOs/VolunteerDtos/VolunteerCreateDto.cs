using Microsoft.AspNetCore.Http;
using System;
using System.ComponentModel.DataAnnotations;
using Utility;

namespace SCCMS.Domain.DTOs.VolunteerDtos
{
	public class VolunteerCreateDto
	{
        [Required]
        [StringLength(50)]
        public string FullName { get; set; }
        [Required]
        [DataType(DataType.Date)]
        public DateTime? DateOfBirth { get; set; }
        [Required]
        public Gender Gender { get; set; }
        [Required]
        [EmailAddress]
        [StringLength(100)]
        public string? Email { get; set; }
        [Required]
        [StringLength(10)]
        public string PhoneNumber { get; set; }
        [Required]
        [StringLength(20)]
        public string NationalId { get; set; }
        [Required]
        public IFormFile NationalImageFront { get; set; }
        [Required]
        public IFormFile NationalImageBack { get; set; }
        [Required]
        public IFormFile Image { get; set; }
        [Required]
        [StringLength(255)]
        public string Address { get; set; }


       
    }
}

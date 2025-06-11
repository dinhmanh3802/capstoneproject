using SCCMS.Domain.DTOs.TeamDtos;
using SCCMS.Infrastucture.Entities;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Utility;

namespace SCCMS.Domain.DTOs.VolunteerCourseDtos
{
    public class VolunteerInforDto
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

        public ProfileStatus Status { get; set; }

        [StringLength(255)]
        public string? Note { get; set; }

        public string Image { get; set; } 
        public string Email { get; set; }
        public string PhoneNumber { get; set; }

        public ICollection<TeamInforDto>? Teams { get; set; }
    }
}

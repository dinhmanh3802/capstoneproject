using SCCMS.Domain.DTOs.StudentGroupDtos;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Utility;

namespace SCCMS.Domain.DTOs.StudentCourseDtos
{
    public class StudentInforDto
    {
        public int? Id { get; set; }

        [Required]
        [StringLength(50)]
        public string FullName { get; set; }


        [DataType(DataType.Date)]
        public DateTime DateOfBirth { get; set; }
        public string Image { get; set; }


        public Gender Gender { get; set; }
        public string ParentName { get; set; }
        public string EmergencyContact { get; set; }
        public string Email { get; set; }
        public string Address { get; set; }
        public string NationalId { get; set; }
        public string NationalImageFront { get; set; }
        public string NationalImageBack { get; set; }
        public string Conduct { get; set; }
        public string AcademicPerformance { get; set; }
        public ICollection<StudentGroupInforDto> StudentGroups { get; set; }
    }
}

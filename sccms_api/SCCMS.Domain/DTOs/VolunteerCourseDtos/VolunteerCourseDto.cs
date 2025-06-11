using SCCMS.Domain.DTOs.VolunteerCourseDtos;
using SCCMS.Infrastucture.Entities;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Utility;

namespace SCCMS.Domain.DTOs.VolunteerApplicationDtos
{
    public class VolunteerCourseDto
    {
        public int Id { get; set; }

        public int CourseId { get; set; }
        public CourseInforDto? Course { get; set; }

        public int VolunteerId { get; set; }
        public VolunteerInforDto Volunteer { get; set; }

        public string? VolunteerCode { get; set; }

        [StringLength(20)]
        public ProgressStatus? Status { get; set; }

        [StringLength(255)]
        public string? Note { get; set; }

        public int? ReviewerId { get; set; }
        public ReviewerInforDto? Reviewer { get; set; }

        [DataType(DataType.Date)]
        public DateTime? ReviewDate { get; set; }

        [DataType(DataType.Date)]
        public DateTime ApplicationDate { get; set; }
        public int? TeamId { get; set; }
        public int SameNationId { get; set; }

    }
}

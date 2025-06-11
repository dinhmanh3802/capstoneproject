using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Utility;

namespace SCCMS.Domain.DTOs.VolunteerCourseDtos
{
    public class VolunteerInformationInACourseDto
    {
        public int VolunteerId { get; set; }
        public int CourseId { get; set; }
        public int TeamId { get; set; }
        public ProgressStatus Status { get; set; }
        public string? Note { get; set; }
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Utility;

namespace SCCMS.Domain.DTOs.VolunteerApplicationDtos
{
    public class VolunteerCourseUpdateDto
    {
        public List<int> Ids { get; set; }
        public ProgressStatus? Status { get; set; }
        public string? Note { get; set; }
        public int? ReviewerId { get; set; }
    }
}

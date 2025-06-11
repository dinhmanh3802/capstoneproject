using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SCCMS.Domain.DTOs.NightShiftDtos
{
    public class NightShiftDto
    {
        public int Id { get; set; }
        public int? CourseId { get; set; }
        public string? CourseName { get; set; }
        public TimeSpan StartTime { get; set; }
        public TimeSpan EndTime { get; set; }
        public string Note { get; set; }
    }
}

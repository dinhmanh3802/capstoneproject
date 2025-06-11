using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SCCMS.Domain.DTOs.StaffFreeTimeDtos
{
    public class StaffFreeTimeCreateDto
    {
        [Required]
        public int UserId { get; set; }

        [Required]
        public int CourseId { get; set; }

        [Required]
        public List<DateTime> FreeDates { get; set; }
    }
}

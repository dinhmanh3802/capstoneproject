using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SCCMS.Domain.DTOs.StaffFreeTimeDtos
{
    public class StaffFreeTimeUpdateDto
    {
        [Required]
        public int UserId { get; set; }

        [Required]
        public int CourseId { get; set; }

        [Required]
        public DateTime Date { get; set; }
    }
}

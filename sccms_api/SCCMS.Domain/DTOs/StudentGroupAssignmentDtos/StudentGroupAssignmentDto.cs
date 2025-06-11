using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SCCMS.Domain.DTOs.StudentGroupAssignmentDtos
{
    public class StudentGroupAssignmentDto
    {
        [Required]
        public List<int> StudentIds { get; set; }

        [Required]
        public int StudentGroupId { get; set; }
        [Required]
        public int CourseId { get; set; }
    }
}

using SCCMS.Infrastucture.Entities;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
    using System.Text;
    using System.Threading.Tasks;
using Utility;

namespace SCCMS.Domain.DTOs.StudentGroupDtos
{
    public class StudentGroupUpdateDto
    {
        public int CourseId { get; set; }
        [StringLength(100)]
        public string GroupName { get; set; }
        public List<int> SupervisorIds { get; set; }
    }
}

using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Utility;

namespace SCCMS.Domain.DTOs.TeamDtos
{
    public class TeamUpdateDto
    {
        public int CourseId { get; set; }

        public int LeaderId { get; set; }

        [StringLength(100)]
        public string TeamName { get; set; }

        public string? Description { get; set; }

        public Gender? Gender { get; set; }
        public int ExpectedVolunteers { get; set; }

    }
}

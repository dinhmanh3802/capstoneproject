using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Utility;

namespace SCCMS.Domain.DTOs.TeamDtos
{
    public class TeamCreateDto
    {
        [Required]
        public int CourseId { get; set; }

        public int LeaderId { get; set; }

        [Required]
        [StringLength(100)]
        public string TeamName { get; set; }

        [StringLength(500)]
        public string? Description { get; set; }

        public Gender? Gender { get; set; }
        public int ExpectedVolunteers { get; set; }
    }
}

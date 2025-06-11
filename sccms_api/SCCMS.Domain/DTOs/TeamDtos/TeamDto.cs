using SCCMS.Domain.DTOs.VolunteerDtos;
using SCCMS.Infrastucture.Entities;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Utility;

namespace SCCMS.Domain.DTOs.TeamDtos
{
    public class TeamDto
    {
		public int Id { get; set; }
    public int? CourseId { get; set; }
    public string CourseName { get; set; }

    public int LeaderId { get; set; }
    public string LeaderName { get; set; }

    [StringLength(100)]
    public string TeamName { get; set; }

    public Gender? Gender { get; set; }
    public int ExpectedVolunteers { get; set; }

    public List<VolunteerInforInTeamDto> Volunteers { get; set; }
	}
}

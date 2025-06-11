using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SCCMS.Domain.DTOs.VolunteerTeamDtos
{
    public class VolunteerTeamDto
    {
        public int TeamId { get; set; }
        public List<int> VolunteerIds { get; set; }
    }
}

using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SCCMS.Infrastucture.Entities
{
    [PrimaryKey(nameof(TeamId), nameof(VolunteerId))]
    public class VolunteerTeam: BaseEntity
    {
        public int TeamId { get; set; }
        public Team Team { get; set; }

        public int VolunteerId { get; set; }
        public Volunteer Volunteer { get; set; }
    }
}

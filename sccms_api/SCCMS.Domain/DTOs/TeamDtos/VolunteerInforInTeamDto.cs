using SCCMS.Domain.DTOs.StudentDtos;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Utility;

namespace SCCMS.Domain.DTOs.TeamDtos
{
    public class VolunteerInforInTeamDto
    {
        public int Id { get; set; }
        public string volunteerCode { get; set; }
        public string FullName { get; set; }
        public DateTime? DateOfBirth { get; set; }
        public Gender? Gender { get; set; }
        public ProgressStatus Status { get; set; }
        public string PhoneNumber { get; set; }

    }
}

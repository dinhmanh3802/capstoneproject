using SCCMS.Domain.DTOs.StudentGroupAssignmentDtos;
using SCCMS.Domain.DTOs.TeamDtos;
using SCCMS.Domain.DTOs.VolunteerTeamDtos;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SCCMS.Domain.Services.Interfaces
{
    public interface IVolunteerTeamService
    {
        Task AddVolunteersIntoTeamAsync(VolunteerTeamDto volunteerTeamDto);

        Task RemoveVolunteersFromTeamAsync(VolunteerTeamDto volunteerTeamDto);
        Task AutoAssignVolunteersToTeamsAsync(int courseId);
    }
}

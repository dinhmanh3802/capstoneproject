using SCCMS.Domain.DTOs.TeamDtos;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Utility;

namespace SCCMS.Domain.Services.Interfaces
{
    public interface ITeamService
    {
        Task CreateTeamAsync(TeamCreateDto entity);
        Task<IEnumerable<TeamDto>> GetAllTeamsByCourseIdAsync(int courseId);
        Task<IEnumerable<VolunteerInforInTeamDto>> GetVolunteersInTeamAsync(int teamId, string? volunteerCode, string? fullName, string? phoneNumber, Gender? gender, ProgressStatus? status);

        Task<TeamDto?> GetTeamByIdAsync(int id);
        Task UpdateTeamAsync(int teamId, TeamUpdateDto entity);
        Task DeleteTeamAsync(int id);
    }
}

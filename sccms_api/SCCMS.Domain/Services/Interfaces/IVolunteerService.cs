using SCCMS.Domain.DTOs.StudentDtos;
using SCCMS.Domain.DTOs.VolunteerDtos;
using System.Collections.Generic;
using System.Threading.Tasks;
using Utility;

namespace SCCMS.Domain.Services.Interfaces
{
	public interface IVolunteerService
	{
		Task<IEnumerable<VolunteerDto>> GetAllVolunteersAsync(
			string? fullName = null,
			Gender? gender = null,
			ProfileStatus? status = null,
			int? teamId = null,
			int? courseId = null,
			string? nationalId = null,
			string? address = null
		);
		Task CreateVolunteerAsync(VolunteerCreateDto volunteerCreateDto, int courseId);
		Task UpdateVolunteerAsync(int volunteerId, VolunteerUpdateDto volunteerUpdateDto);
		Task<VolunteerDto> GetVolunteerByIdAsync(int volunteerId);
        Task<IEnumerable<VolunteerDto>> GetVolunteersByCourseIdAsync(int courseId);
		Task<byte[]> ExportVolunteersByCourseAsync(int courseId);
		Task<byte[]> ExportVolunteersByTeamAsync(int teamId);
		Task SendEmailsToVolunteersAsync(int? courseId, string subject, string templateName, Dictionary<string, string> additionalParameters = null);

	}
}

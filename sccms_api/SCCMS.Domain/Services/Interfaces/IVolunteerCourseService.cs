using SCCMS.Domain.DTOs.StudentCourseDtos;
using SCCMS.Domain.DTOs.VolunteerApplicationDtos;
using SCCMS.Domain.DTOs.VolunteerCourseDtos;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Utility;

namespace SCCMS.Domain.Services.Interfaces
{
    public interface IVolunteerCourseService
    {
        Task<IEnumerable<VolunteerCourseDto?>> GetVolunteerApplicationAsync(int courseId, string? name = null, Gender? gender = null,  string? phoneNumber = null, ProgressStatus? status = null, int? reviewerId = null,
                                                        DateTime? startDob = null, DateTime? endDob = null, int? teamId = null, string? volunteerCode = null, string? nationalId= null);
        Task<IEnumerable<VolunteerCourseDto?>> GetAllVolunteerCourseAsync(int courseId, string? name = null, Gender? gender = null, string? teamName = null, string? phoneNumber = null, ProgressStatus? status = null, string? volunteerCode = null,
                                               DateTime? startDob = null, DateTime? endDob = null);

        Task<VolunteerCourseDto?> GetVolunteerCourseByIdAsync(int id);
        Task UpdateVolunteerCourseAsync(VolunteerCourseUpdateDto volunteerCourseDto);
        Task AutoAssignApplicationsAsync(int courseId);
        
        Task<VolunteerCourseDto?> GetByVolunteerIdAndCourseIdAsync(int volunteerId, int courseId);
        Task SendApplicationResultAsync(int[] listVolunteerApplicationId, int courseId, string subject, string body);
        Task SendVolunteerApplicationResultAsync(int[] listVolunteerApplicationId, int courseId, string subject, string body);
        Task<byte[]> GenerateVolunteerCardsPdfAsync(List<int> volunteerCourseIds, int courseId);
        Task<byte[]> GenerateVolunteerCertificatePdfAsync(List<int> volunteerCourseIds, int courseId);

        Task UpdateVolunteerInformationInACourseAsync(VolunteerInformationInACourseDto volunteerInfoDto);
    }
}

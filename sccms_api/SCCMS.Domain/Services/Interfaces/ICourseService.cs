using SCCMS.Domain.DTOs.CourseDtos;
using SCCMS.Domain.DTOs.DashboardDtos;
using System.Collections.Generic;
using System.Threading.Tasks;
using Utility;

namespace SCCMS.Domain.Services.Interfaces
{
    public interface ICourseService
    {
        Task<IEnumerable<CourseDto>> GetAllCoursesAsync(string? courseName= null, CourseStatus? status = null,
                                                        DateTime? startDate = null, DateTime? endDate = null);
        Task<IEnumerable<CourseDto>> GetAvaiableFeedBackCourseAsync();
        Task<CourseDto?> GetCourseByIdAsync(int id);
        Task CreateCourseAsync(CourseCreateDto courseCreateDto);
        Task UpdateCourseAsync(int courseId, CourseUpdateDto courseUpdateDto);
        Task DeleteCourseAsync(int id);
        Task<CourseDto?> GetCurrentCourseAsync();
        Task<DashboardDto> GetCourseDashboardDataAsync(int courseId);
        Task<List<RegistrationPerCourseDto>> GetStudentRegistrationsPerCourseAsync(int years);
        Task<List<RegistrationPerCourseDto>> GetVolunteerRegistrationsPerCourseAsync(int years);
    }
}

using Microsoft.EntityFrameworkCore;
using SCCMS.Domain.DTOs.CourseDtos;

using SCCMS.Domain.DTOs.StudentCourseDtos;
using SCCMS.Infrastucture.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Utility;

namespace SCCMS.Domain.Services.Interfaces
{
    public interface IStudentApplicationService
    {
        Task<IEnumerable<StudentCourseDto>> GetAllStudentApplicationAsync(int? courseId = null, string? studentName = null, Gender? gender = null, string? parentName = null, string? phoneNumber = null, ProgressStatus? status = null, int? reviewerId = null,
                                                        DateTime? startDob = null, DateTime? endDob = null, string? nationalId= null);
        Task<IEnumerable<StudentCourseDto>> GetAllStudentCourseAsync(int courseId, string? studentName = null, Gender? gender = null, string? studentGroupName = null, string? phoneNumber = null, ProgressStatus? status = null, string? studentCode = null,
                                                DateTime? startDob = null, DateTime? endDob = null, int? StudentGroup = null, int? StudentGroupExcept = null, bool? isGetStudentDrop= true);
        Task<StudentCourseDto?> GetStudentApplicationByIdAsync(int id);
        Task CreateStudentApplicationAsync(StudentCourseDto studentApplicationCreateDto);
        Task UpdateStatusStudentApplicationAsync(StudentCourseUpdateDto studentApplicationDto);
        Task UpdateStatusStudentApplicationDetailAsync(int courseId, int applicationId, string note, ProgressStatus status, int studentGroupId);
        Task DeleteStudentApplicationAsync(int id);
        Task AutoAssignApplicationsAsync(int courseId);
        Task SendApplicationResultAsync(int[] listStudentApplicaionId, int courseId, string subject, string body);
        Task<StudentCourseDto?> GetByStudentIdAndCourseIdAsync(int studentId, int courseId);
        Task<byte[]> GenerateStudentCardsPdfAsync(List<int> studentCourseIds, int courseId);

    }
}

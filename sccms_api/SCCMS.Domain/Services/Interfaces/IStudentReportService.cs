// Domain/Services/Interfaces/IStudentReportService.cs
using SCCMS.Domain.DTOs.StudentReportDtos;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace SCCMS.Domain.Services.Interfaces
{
    public interface IStudentReportService
    {
        Task<IEnumerable<StudentReportDto>> GetAttendanceByGroupAsync(int studentGroupId, int reportId);
        Task MarkAttendanceAsync(int studentGroupId, int reportId, List<MarkAttendanceRequestDto> attendanceData, int supervisorId);
        Task ReopenReportAsync(int reportId, int managerId);
        Task MarkReportAsReadAsync(int reportId, int managerId);
        Task<StudentReportDto?> GetStudentReportByIdAsync(int id);
    }
}

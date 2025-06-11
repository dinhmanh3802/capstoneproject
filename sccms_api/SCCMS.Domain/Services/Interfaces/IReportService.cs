using SCCMS.Domain.DTOs.ReportDtos;
using SCCMS.Domain.DTOs.StudentReportDtos;
using Utility;

namespace SCCMS.Domain.Services
{
    public interface IReportService
    {
        Task<IEnumerable<ReportDto>> GetReportsAsync(
        ReportType reportType,
        DateTime? startDate = null,
        DateTime? endDate = null,
        ReportStatus? status = null,
        int? courseId = null,
        int? groupId = null,
        int? roomId = null
    );
        Task SubmitAttendanceReportAsync(int reportId, int supervisorId, List<StudentReportDto> studentReports, string reportContent);
        Task<IEnumerable<AttendanceSummaryDto>> GetAttendanceSummariesAsync(
            ReportType reportType,
            int courseId,
            DateTime? startDate = null,
            DateTime? endDate = null
        );
        Task<IEnumerable<ReportDto>> GetReportAsync(
        int? reportId,
        ReportType? reportType,
        DateTime? reportDate,
        ReportStatus? status,
        int? courseId,
        int? groupId,
        int? roomId,
        int? nightShiftId,
        int? currentUserId,
        int? currentUserRoleId
    );
        Task<IEnumerable<ReportDto>> GetAttendanceReportsByDateAsync(int courseId, DateTime? reportDate = null, ReportStatus? status = null, int? studentGroupId = null);
        Task<IEnumerable<StudentReportViewDto>> GetReportsByStudentAsync(int courseId, string? StudentCode=null , int? studentId= null);
        Task MarkReportAsReadAsync(int reportId);
        Task ReopenReportAsync(int reportId);
        Task UpdateReportStatusesAsync();
        Task RequestReopenReportAsync(int reportId, int supervisorId);
        Task SubmitNightShiftReportAsync(int reportId, int staffId, List<StudentReportDto> studentReports, string reportContent);
    }
}

// SCCMS.API.Controllers.ReportController.cs

using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SCCMS.Domain.DTOs.ReportDtos;
using SCCMS.Domain.DTOs.StudentReportDtos;
using SCCMS.Domain.Services;
using SCCMS.Domain.Services.Interfaces;
using SCCMS.Infrastucture.Entities;
using System.Net;
using Utility;

namespace SCCMS.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ReportController : ControllerBase
    {
        private readonly IReportService _reportService;
        private readonly IUserService _userService; // Service để lấy thông tin người dùng

        public ReportController(IReportService reportService, IUserService userService)
        {
            _reportService = reportService;
            _userService = userService;
        }


        [HttpGet("report")]
        [Authorize]
        public async Task<IActionResult> GetReport(
    [FromQuery] int? reportId = null,
    [FromQuery] ReportType? reportType = null,
    [FromQuery] DateTime? reportDate = null,
    [FromQuery] ReportStatus? status = null,
    [FromQuery] int? courseId = null,
    [FromQuery] int? groupId = null,
    [FromQuery] int? roomId = null,
    [FromQuery] int? nightShiftId = null
)
        {
            if (!reportId.HasValue && !reportType.HasValue)
            {
                return BadRequest(new ApiResponse(
                    HttpStatusCode.BadRequest,
                    false,
                    new List<string> { "Bạn cần cung cấp ít nhất reportId hoặc reportType." }
                ));
            }

            var userIdClaim = User.FindFirst("userId")?.Value;
            var userRoleIdClaim = User.FindFirst("roleId")?.Value;
            var userId = int.Parse(userIdClaim);
            var userRoleId = int.Parse(userRoleIdClaim);

            var reports = await _reportService.GetReportAsync(
                reportId,
                reportType,
                reportDate,
                status,
                courseId,
                groupId,
                roomId,
                nightShiftId,
                userId,
                userRoleId
            );

            if (!reports.Any())
            {
                return NotFound(new ApiResponse(
                    HttpStatusCode.NotFound,
                    false,
                    new List<string> { "Không tìm thấy báo cáo phù hợp." }
                ));
            }

            return Ok(new ApiResponse(HttpStatusCode.OK, true, reports));
        }
        
        // Supervisor: Nộp báo cáo điểm danh
        [HttpPost("supervisor/report/{reportId}")]
        public async Task<IActionResult> SubmitAttendanceReport(int reportId, [FromBody] SubmitAttendanceReportDto submitDto)
        {
            var supervisorId = int.Parse(User.FindFirst("userId").Value);
            try
            {
                await _reportService.SubmitAttendanceReportAsync(reportId, supervisorId, submitDto.StudentReports, submitDto.ReportContent);
                return Ok(new ApiResponse(HttpStatusCode.OK, true, "Nộp báo cáo thành công"));
            }
            catch (UnauthorizedAccessException ex)
            {
                return Forbid();
            }
            catch (Exception ex)
            {
                return BadRequest(new ApiResponse(HttpStatusCode.BadRequest, false, new List<string> { ex.Message }));
            }
        }



        // Manager: Lấy báo cáo điểm danh theo ngày
        [HttpGet("attendance-reports/{courseId}")]
        [Authorize]
        public async Task<IActionResult> GetAttendanceReportsByDate(int courseId,
            [FromQuery] DateTime? reportDate = null,
            [FromQuery] ReportStatus? status = null,
            [FromQuery] int? studentGroupId = null)
        {
            var reports = await _reportService.GetAttendanceReportsByDateAsync(courseId, reportDate, status, studentGroupId);
            return Ok(new ApiResponse(HttpStatusCode.OK, true, reports));
        }


        // Manager: Đánh dấu báo cáo đã đọc
        [HttpPost("manager/mark-as-read/{reportId}")]
        [Authorize]
        public async Task<IActionResult> MarkReportAsRead(int reportId)
        {
            try
            {
                await _reportService.MarkReportAsReadAsync(reportId);
                return Ok(new ApiResponse(HttpStatusCode.OK, true, "Đã đánh dấu báo cáo là đã đọc"));
            }
            catch (Exception ex)
            {
                return BadRequest(new ApiResponse(HttpStatusCode.BadRequest, false, new List<string> { ex.Message }));
            }
        }

        // Manager: Mở lại báo cáo
        [HttpPost("manager/reopen-report/{reportId}")]
        [Authorize]
        public async Task<IActionResult> ReopenReport(int reportId)
        {
            try
            {
                await _reportService.ReopenReportAsync(reportId);
                return Ok(new ApiResponse(HttpStatusCode.OK, true, "Đã mở lại báo cáo"));
            }
            catch (Exception ex)
            {
                return BadRequest(new ApiResponse(HttpStatusCode.BadRequest, false, new List<string> { ex.Message }));
            }
        }

        // Supervisor: Yêu cầu mở lại báo cáo
        [HttpPost("request-reopen/{reportId}")]
        public async Task<IActionResult> RequestReopenReport(int reportId)
        {
            var supervisorIdClaim = User.FindFirst("userId")?.Value;
            if (supervisorIdClaim == null)
            {
                return Unauthorized(new ApiResponse(HttpStatusCode.Unauthorized, false, new List<string> { "User not authenticated." }));
            }

            var supervisorId = int.Parse(supervisorIdClaim);
            try
            {
                await _reportService.RequestReopenReportAsync(reportId, supervisorId);
                return Ok(new ApiResponse(HttpStatusCode.OK, true, "Yêu cầu mở lại báo cáo đã được gửi."));
            }
            catch (UnauthorizedAccessException)
            {
                return Forbid();
            }
            catch (Exception ex)
            {
                return BadRequest(new ApiResponse(HttpStatusCode.BadRequest, false, new List<string> { ex.Message }));
            }
        }





        // Staff: Nộp báo cáo trực đêm
        [HttpPost("staff/nightshift-report/{reportId}")]
        public async Task<IActionResult> SubmitNightShiftReport(int reportId, [FromBody] SubmitAttendanceReportDto submitDto)
        {
            var staffId = int.Parse(User.FindFirst("userId").Value);
            try
            {
                await _reportService.SubmitNightShiftReportAsync(reportId, staffId, submitDto.StudentReports, submitDto.ReportContent);
                return Ok(new ApiResponse(HttpStatusCode.OK, true, "Nộp báo cáo thành công"));
            }
            catch (UnauthorizedAccessException)
            {
                return Forbid();
            }
            catch (Exception ex)
            {
                return BadRequest(new ApiResponse(HttpStatusCode.BadRequest, false, new List<string> { ex.Message }));
            }
        }


        [HttpGet("student-reports-view")]
        [Authorize]
        public async Task<IActionResult> GetReportsByStudent(
            [FromQuery] int courseId,
            [FromQuery] string? studentCode,
            [FromQuery] int? studentId
        )
        {

            if (courseId <= 0)
            {
                return BadRequest(new ApiResponse(
                    HttpStatusCode.BadRequest,
                    false,
                    new List<string> { "CourseId phải lớn hơn 0." }
                ));
            }

            try
            {
                var reports = await _reportService.GetReportsByStudentAsync(courseId, studentCode, studentId);

                if (reports == null || !reports.Any())
                {
                    return NotFound(new ApiResponse(
                        HttpStatusCode.NotFound,
                        false,
                        new List<string> { "Không tìm thấy báo cáo cho học sinh này trong khóa học đã chọn." }
                    ));
                }

                return Ok(new ApiResponse(HttpStatusCode.OK, true, reports));
            }
            catch (InvalidOperationException ex)
            {
                // Thường xảy ra khi courseId không tồn tại hoặc studentCode không thuộc khóa học
                return BadRequest(new ApiResponse(
                    HttpStatusCode.BadRequest,
                    false,
                    new List<string> { ex.Message }
                ));
            }
            catch (Exception ex)
            {
                // Xử lý các lỗi khác
                return StatusCode((int)HttpStatusCode.InternalServerError, new ApiResponse(
                    HttpStatusCode.InternalServerError,
                    false,
                    new List<string> { "Đã xảy ra lỗi.", ex.Message }
                ));
            }
        }
    }
}

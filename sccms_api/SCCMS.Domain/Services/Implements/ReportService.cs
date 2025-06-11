using AutoMapper;
using DocumentFormat.OpenXml.Bibliography;
using SCCMS.Domain.DTOs.FeedbackDtos;
using SCCMS.Domain.DTOs.ReportDtos;
using SCCMS.Domain.DTOs.StudentReportDtos;
using SCCMS.Domain.Services.Interfaces;
using SCCMS.Infrastucture.Entities;
using SCCMS.Infrastucture.UnitOfWork;
using System.Linq.Expressions;
using Utility;

namespace SCCMS.Domain.Services.Implements
{
    public class ReportService : IReportService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;
        private readonly INotificationService _notificationService;

        public ReportService(IUnitOfWork unitOfWork, IMapper mapper, INotificationService notificationService)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
            _notificationService = notificationService;
        }

        public async Task<IEnumerable<ReportDto>> GetReportsAsync(
            ReportType reportType,
            DateTime? startDate = null,
            DateTime? endDate = null,
            ReportStatus? status = null,
            int? courseId = null,
            int? groupId = null,
            int? roomId = null
        )
        {
            // Xây dựng biểu thức lọc
            Expression<Func<Report, bool>> predicate = r =>
                r.ReportType == reportType &&
                (!startDate.HasValue || r.ReportDate.Date >= startDate.Value.Date) &&
                (!endDate.HasValue || r.ReportDate.Date <= endDate.Value.Date) &&
                (!status.HasValue || r.Status == status.Value) &&
                (!courseId.HasValue || r.CourseId == courseId.Value) &&
                (!groupId.HasValue || r.StudentGroupId == groupId.Value) &&
                (!roomId.HasValue || r.RoomId == roomId.Value);

            // Lấy danh sách báo cáo
            var reports = await _unitOfWork.Report.FindAsync(
                predicate,
                includeProperties: "StudentGroup,Room,NightShift,StudentReports.Student"
            );

            var reportDtos = _mapper.Map<IEnumerable<ReportDto>>(reports);

            return reportDtos;
        }

        public async Task<IEnumerable<ReportDto>> GetReportAsync(
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
        )
        {
            // Xây dựng biểu thức lọc
            Expression<Func<Report, bool>> predicate = r =>
                (!reportId.HasValue || r.Id == reportId.Value) &&
                (!reportType.HasValue || r.ReportType == reportType.Value) &&
                (!reportDate.HasValue || r.ReportDate.Date == reportDate.Value.Date) &&
                (!status.HasValue || r.Status == status.Value) &&
                (!courseId.HasValue || r.CourseId == courseId.Value) &&
                (!groupId.HasValue || r.StudentGroupId == groupId.Value) &&
                (!roomId.HasValue || r.RoomId == roomId.Value) &&
                (!nightShiftId.HasValue || r.NightShiftId == nightShiftId.Value);

            // Lấy danh sách báo cáo với các thuộc tính cần thiết
            var reports = await _unitOfWork.Report.FindAsync(
                predicate,
                includeProperties: "StudentGroup.SupervisorStudentGroup,Room,NightShift,StudentReports.Student.StudentCourses,SubmittedByUser"
            );

            var reportDtos = _mapper.Map<IEnumerable<ReportDto>>(reports);

            foreach (var reportDto in reportDtos)
            {
                // Xác định IsEditable cho từng báo cáo
                reportDto.IsEditable = await IsReportEditableByUser(
                    reportDto,
                    currentUserId.Value,
                    currentUserRoleId.Value
                );

                // Xác định phân công cho Supervisor
                if (currentUserRoleId == SD.RoleId_Supervisor)
                {
                    reportDto.IsSupervisorAssigned = await _unitOfWork.SupervisorStudentGroup
                        .AnyAsync(ssg => ssg.SupervisorId == currentUserId &&
                                         ssg.StudentGroupId == reportDto.StudentGroupId);
                }

                // Xác định phân công cho Staff
                if (currentUserRoleId == SD.RoleId_Staff)
                {
                    reportDto.IsStaffAssigned = await _unitOfWork.NightShiftAssignment
                        .AnyAsync(nsa => nsa.UserId == currentUserId &&
                                         nsa.NightShiftId == reportDto.NightShiftId &&
                                         nsa.RoomId == reportDto.RoomId &&
                                         nsa.Date.Date == reportDto.ReportDate.Date);
                }

                // Cập nhật StudentCode cho từng StudentReportDto
                foreach (var studentReport in reportDto.StudentReports)
                {
                    // Tìm StudentCourse tương ứng với CourseId của báo cáo
                    var studentCourse = reports
                        .FirstOrDefault(r => r.Id == reportDto.Id)?
                        .StudentReports
                        .FirstOrDefault(sr => sr.StudentId == studentReport.StudentId)?
                        .Student.StudentCourses
                        .FirstOrDefault(sc => sc.CourseId == reportDto.CourseId);

                    studentReport.StudentCode = studentCourse?.StudentCode;
                }
            }

            return reportDtos;
        }



        private async Task<bool> IsReportEditableByUser(
            ReportDto reportDto,
            int userId,
            int userRoleId
        )
        {
            // Kiểm tra nếu trạng thái là NotYet và ngày báo cáo là ngày hôm nay
            bool isReportDateToday = reportDto.ReportDate.Date == DateTime.Now.Date;

            if ((reportDto.Status == ReportStatus.NotYet && isReportDateToday) ||
                reportDto.Status == ReportStatus.Attending ||
                reportDto.Status == ReportStatus.Reopened)
            {
                if (userRoleId == SD.RoleId_Supervisor &&
                    reportDto.ReportType == ReportType.DailyReport)
                {
                    // Kiểm tra xem Supervisor có quản lý nhóm này không
                    var isSupervisorAssigned = await _unitOfWork.SupervisorStudentGroup
                        .AnyAsync(ssg => ssg.SupervisorId == userId &&
                                         ssg.StudentGroupId == reportDto.StudentGroupId);

                    return isSupervisorAssigned;
                }
                else if (userRoleId == SD.RoleId_Staff &&
                         reportDto.ReportType == ReportType.nightShift)
                {
                    // Kiểm tra xem Staff có được gán vào ca trực này không
                    var isStaffAssigned = await _unitOfWork.NightShiftAssignment
                        .AnyAsync(nsa => nsa.UserId == userId &&
                                         nsa.NightShiftId == reportDto.NightShiftId &&
                                         nsa.RoomId == reportDto.RoomId &&
                                         nsa.Date.Date == reportDto.ReportDate.Date && nsa.Status != NightShiftAssignmentStatus.cancelled && nsa.Status != NightShiftAssignmentStatus.rejected);

                    return isStaffAssigned;
                }
            }

            return false;
        }

        // Hàm lấy tổng kết báo cáo theo phạm vi ngày
        public async Task<IEnumerable<AttendanceSummaryDto>> GetAttendanceSummariesAsync(
            ReportType reportType,
            int courseId,
            DateTime? startDate = null,
            DateTime? endDate = null
        )
        {
            // Lấy danh sách báo cáo theo khóa tu và phạm vi ngày
            var reports = await _unitOfWork.Report.FindAsync(
                r => r.CourseId == courseId &&
                     r.ReportType == reportType &&
                     (!startDate.HasValue || r.ReportDate.Date >= startDate.Value.Date) &&
                     (!endDate.HasValue || r.ReportDate.Date <= endDate.Value.Date),
                includeProperties: "StudentGroup,Room,NightShift,StudentReports"
            );

            // Nhóm theo ngày
            var summaries = reports.GroupBy(r => r.ReportDate.Date)
                .Select(g => new AttendanceSummaryDto
                {
                    Date = g.Key,
                    TotalStudents = g.Sum(r => r.StudentReports.Count()),
                    TotalPresent = g.Sum(r => r.StudentReports.Count(sr => sr.Status == StudentReportStatus.Present))
                });

            return summaries;
        }

       
        private bool IsCourseEditable(Course course)
        {
            return course.Status == CourseStatus.inProgress;
        }

        public async Task SubmitAttendanceReportAsync(
    int reportId,
    int supervisorId,
    List<StudentReportDto> studentReports,
    string reportContent
)
        {
            // Kiểm tra báo cáo tồn tại
            var report = await _unitOfWork.Report.GetAsync(
                r => r.Id == reportId && r.ReportType == ReportType.DailyReport,
                includeProperties: "StudentGroup"
            );

            if (report == null)
                throw new ArgumentException("Không tìm thấy báo cáo.");

            // Kiểm tra quyền
            var isSupervisorAssigned = await _unitOfWork.SupervisorStudentGroup
                .AnyAsync(ssg => ssg.SupervisorId == supervisorId &&
                                 ssg.StudentGroupId == report.StudentGroupId);

            if (!isSupervisorAssigned)
                throw new UnauthorizedAccessException("Bạn không có quyền chỉnh sửa báo cáo này.");

            // Kiểm tra trạng thái báo cáo
            if (report.Status != ReportStatus.NotYet &&
                report.Status != ReportStatus.Attending &&
                report.Status != ReportStatus.Reopened)
            {
                throw new InvalidOperationException("Không thể chỉnh sửa báo cáo ở trạng thái hiện tại.");
            }
            if (report.Status == ReportStatus.NotYet)
            {
                report.Status = ReportStatus.Attending;
            } else if (report.Status == ReportStatus.Reopened)
            {
                report.Status = ReportStatus.Attended;
                var message = $"Báo cáo ngày {report.ReportDate.ToString("dd/MM/yyyy")} đã được chỉnh sửa và nộp lại. Vui lòng kiểm tra.";
                var link = $"/report/{report.Id}";
                await _notificationService.NotifyUserAsync(2, message, link);
            }
            // Cập nhật báo cáo
            report.ReportContent = reportContent;
            report.SubmissionDate = DateTime.Now;
            report.SubmissionBy = supervisorId;

            // Cập nhật StudentReports
            foreach (var studentReportDto in studentReports)
            {
                var studentReport = await _unitOfWork.StudentReport.GetAsync(
                    sr => sr.ReportId == reportId &&
                          sr.StudentId == studentReportDto.StudentId
                );

                if (studentReport == null)
                {
                    studentReport = new StudentReport
                    {
                        ReportId = reportId,
                        StudentId = studentReportDto.StudentId,
                        Status = studentReportDto.Status,
                        Comment = studentReportDto.Comment
                    };
                    await _unitOfWork.StudentReport.AddAsync(studentReport);
                }
                else
                {
                    studentReport.Status = studentReportDto.Status;
                    studentReport.Comment = studentReportDto.Comment;
                    await _unitOfWork.StudentReport.UpdateAsync(studentReport);
                }
            }


            await _unitOfWork.Report.UpdateAsync(report);
            await _unitOfWork.SaveChangeAsync();
        }

        private bool IsReportEditable(Report report)
        {
            var now = DateTime.Now;
            if (report.Status == ReportStatus.NotYet || report.Status == ReportStatus.Attending || report.Status == ReportStatus.Reopened)
            {

                return true;
            }
            return false;
        }



        public async Task<IEnumerable<ReportDto>> GetAttendanceReportsByDateAsync(int courseId, DateTime? reportDate = null, ReportStatus? status = null, int? studentGroupId = null)
        {
            var reports = await _unitOfWork.Report.FindAsync(
                r => r.CourseId == courseId &&
                     r.ReportType == ReportType.DailyReport &&
                     (!reportDate.HasValue || r.ReportDate.Date == reportDate.Value.Date) &&
                     (!status.HasValue || r.Status == status.Value) &&
                     (!studentGroupId.HasValue || r.StudentGroupId == studentGroupId),
                includeProperties: "StudentGroup,StudentReports.Student"
            );

            var reportDtos = _mapper.Map<IEnumerable<ReportDto>>(reports);
            return reportDtos;
        }


        public async Task MarkReportAsReadAsync(int reportId)
        {
            var report = await _unitOfWork.Report.GetAsync(
                r => r.Id == reportId,
                includeProperties: "Course"
            );

            if (report == null)
                throw new ArgumentException("Không tìm thấy báo cáo.");

            // Kiểm tra trạng thái khóa tu
            if (!IsCourseEditable(report.Course))
                throw new InvalidOperationException("Chỉ có thể đánh dấu báo cáo trong khóa tu đang diễn ra.");

            if (report.Status != ReportStatus.Attended)
                throw new InvalidOperationException("Chỉ có thể đánh dấu là đã đọc cho các báo cáo đã nộp.");

            report.Status = ReportStatus.Read;
            await _unitOfWork.Report.UpdateAsync(report);
            await _unitOfWork.SaveChangeAsync();
        }


        public async Task ReopenReportAsync(int reportId)
        {
            var report = await _unitOfWork.Report.GetAsync(
                r => r.Id == reportId,
                includeProperties: "Course,StudentGroup"
            );

            if (report == null)
                throw new ArgumentException("Không tìm thấy báo cáo.");

            // Kiểm tra trạng thái khóa tu
            if (!IsCourseEditable(report.Course))
                throw new InvalidOperationException("Chỉ có thể mở lại báo cáo trong khóa tu đang diễn ra.");

            report.Status = ReportStatus.Reopened;
            await _unitOfWork.Report.UpdateAsync(report);
            await _unitOfWork.SaveChangeAsync();

            if (report.ReportType == ReportType.DailyReport)
            {
                // Gửi thông báo cho Supervisor
                var supervisors = await _unitOfWork.SupervisorStudentGroup.FindAsync(
                    ssg => ssg.StudentGroupId == report.StudentGroupId
                );

                foreach (var supervisor in supervisors)
                {
                    string message = $"Báo cáo điểm danh ngày {report.ReportDate:dd/MM/yyyy} đã được mở lại.";
                    string link = "report/" + report.Id;
                    await _notificationService.NotifyUserAsync(supervisor.SupervisorId, message, link);
                }
            }
            if (report.ReportType == ReportType.nightShift)
            {
                // Gửi thông báo cho Staffs
                var staffs = await _unitOfWork.NightShiftAssignment.FindAsync(
                ssg => ssg.NightShiftId == report.NightShiftId && ssg.Date == report.ReportDate && ssg.RoomId == report.RoomId);

                foreach (var staff in staffs)
                {
                    string message = $"Báo cáo trực đêm ngày {report.ReportDate:dd/MM/yyyy} đã được mở lại.";
                    string link = "report/" + report.Id;
                    await _notificationService.NotifyUserAsync((int)staff.UserId, message, link);
                }
            }
        }


        public async Task UpdateReportStatusesAsync()
        {
            var today = DateTime.Now.Date;

            // Lấy tất cả các báo cáo thuộc ngày trong quá khứ
            var reports = await _unitOfWork.Report.FindAsync(
                r => r.ReportDate < today && // Báo cáo thuộc ngày trước hôm nay
                     (r.Status == ReportStatus.NotYet ||
                      r.Status == ReportStatus.Attending ||
                      r.Status == ReportStatus.Reopened)
            );

            foreach (var report in reports)
            {
                if (report.Status == ReportStatus.NotYet || report.Status == ReportStatus.Reopened)
                {
                    // Nếu báo cáo chưa xử lý hoặc bị mở lại, đánh dấu là trễ hạn
                    report.Status = ReportStatus.Late;
                    var message = $"Báo cáo ngày {report.ReportDate.ToString("dd/MM/yyyy")} chưa được hoàn thành. Vui lòng kiểm tra và nhắc nhở người phụ trách.";
                    var link = $"/report/{report.Id}";
                    await _notificationService.NotifyUserAsync(2, message, link);
                }
                else if (report.Status == ReportStatus.Attending)
                {
                    // Nếu báo cáo đang được xử lý, đánh dấu là đã xử lý xong
                    report.Status = ReportStatus.Attended;
                }

                await _unitOfWork.Report.UpdateAsync(report);
            }

            // Lưu thay đổi vào cơ sở dữ liệu
            await _unitOfWork.SaveChangeAsync();
        }


        public async Task RequestReopenReportAsync(int reportId, int userId)
        {
            // Lấy báo cáo từ cơ sở dữ liệu
            var report = await _unitOfWork.Report.GetAsync(
                r => r.Id == reportId
            );

            if (report == null)
                throw new ArgumentException("Không tìm thấy báo cáo.");

            // Kiểm tra quyền truy cập của Supervisor đối với báo cáo này
            var hasAccessSupervisor = await _unitOfWork.SupervisorStudentGroup.GetAsync(
                ssg => ssg.SupervisorId == userId && ssg.StudentGroupId == report.StudentGroupId
            );
            var hasAccessStaff = await _unitOfWork.NightShiftAssignment.GetAsync(
                ssg => ssg.UserId == userId && ssg.NightShiftId == report.NightShiftId);

            if (hasAccessSupervisor == null && hasAccessStaff == null)
                throw new UnauthorizedAccessException("Bạn không có quyền truy cập báo cáo này.");

            // Kiểm tra trạng thái báo cáo hiện tại
            if (report.Status == ReportStatus.Reopened || report.Status == ReportStatus.NotYet || report.Status == ReportStatus.Attending)
                throw new InvalidOperationException("Báo cáo đã được mở lại hoặc đang chờ mở lại.");


            // Lấy manager's id theo role "Manager"
            var manager = await _unitOfWork.User
                .FindAsync(u => u.RoleId == SD.RoleId_Manager);

            if (manager == null)
                throw new InvalidOperationException("Không tìm thấy Manager trong hệ thống.");

            var managerId = manager.FirstOrDefault().Id;


            // Gửi thông báo yêu cầu mở lại đến Manager sử dụng NotificationService
            var message = $"Huynh trưởng đã yêu cầu mở lại báo cáo ngày {report.ReportDate.ToString("dd/MM/yyyy")}.";
            var link = $"/report/{report.Id}";
            await _notificationService.NotifyUserAsync(2, message, link);

            await _unitOfWork.Report.UpdateAsync(report);
            await _unitOfWork.SaveChangeAsync();
        }

        public async Task SubmitNightShiftReportAsync(
    int reportId,
    int staffId,
    List<StudentReportDto> studentReports,
    string reportContent
)
        {
            // Kiểm tra báo cáo tồn tại
            var report = await _unitOfWork.Report.GetAsync(
                r => r.Id == reportId && r.ReportType == ReportType.nightShift,
                includeProperties: "Room,NightShift"
            );

            if (report == null)
                throw new ArgumentException("Không tìm thấy báo cáo.");

            // Kiểm tra quyền
            var isStaffAssigned = await _unitOfWork.NightShiftAssignment
                .FindAsync(nsa => nsa.UserId == staffId &&
                                 nsa.NightShiftId == report.NightShiftId &&
                                 nsa.RoomId == report.RoomId &&
                                 nsa.Date.Date == report.ReportDate.Date);

            if (!isStaffAssigned.Any())
                throw new UnauthorizedAccessException("Bạn không có quyền chỉnh sửa báo cáo này.");

            // Kiểm tra trạng thái báo cáo
            if (report.Status != ReportStatus.NotYet &&
                report.Status != ReportStatus.Attending &&
                report.Status != ReportStatus.Reopened)
            {
                throw new InvalidOperationException("Không thể chỉnh sửa báo cáo ở trạng thái hiện tại.");
            }

            if (report.Status == ReportStatus.NotYet)
            {
                report.Status = ReportStatus.Attending;
            }
            else if (report.Status == ReportStatus.Reopened)
            {
                report.Status = ReportStatus.Attended;
                var message = $"Báo cáo ngày {report.ReportDate.ToString("dd/MM/yyyy")} đã được chỉnh sửa và nộp lại. Vui lòng kiểm tra.";
                var link = $"/report/{report.Id}";
                await _notificationService.NotifyUserAsync(2, message, link);
            }

            // Cập nhật báo cáo
            report.ReportContent = reportContent;
            report.SubmissionDate = DateTime.Now;
            report.SubmissionBy = staffId;

            // Cập nhật StudentReports
            foreach (var studentReportDto in studentReports)
            {
                var studentReport = await _unitOfWork.StudentReport.GetAsync(
                    sr => sr.ReportId == reportId &&
                          sr.StudentId == studentReportDto.StudentId
                );

                if (studentReport == null)
                {
                    studentReport = new StudentReport
                    {
                        ReportId = reportId,
                        StudentId = studentReportDto.StudentId,
                        Status = studentReportDto.Status,
                        Comment = studentReportDto.Comment
                    };
                    await _unitOfWork.StudentReport.AddAsync(studentReport);
                }
                else
                {
                    studentReport.Status = studentReportDto.Status;
                    studentReport.Comment = studentReportDto.Comment;
                    await _unitOfWork.StudentReport.UpdateAsync(studentReport);
                }
            }

            var nightShiftAssignment = await _unitOfWork.NightShiftAssignment
                 .FindAsync(nsa => nsa.NightShiftId == report.NightShiftId && nsa.RoomId == report.RoomId && nsa.Date == report.ReportDate);
            if (nightShiftAssignment != null)
            {
                foreach (var nsa in nightShiftAssignment)
                {
                    if (nsa.Status == NightShiftAssignmentStatus.notStarted)
                    {
                        nsa.Status = NightShiftAssignmentStatus.completed;
                    }
                    await _unitOfWork.NightShiftAssignment.UpdateAsync(nsa);
                }
            }

            await _unitOfWork.Report.UpdateAsync(report);
            await _unitOfWork.SaveChangeAsync();
        }


       
        public async Task<IEnumerable<StudentReportViewDto>> GetReportsByStudentAsync(int CourseId, string? StudentCode = null, int? studentId = null)
        {
            if (StudentCode != null)
            {
                if (CourseId <= 0)
                {
                    throw new Exception("CourseId phải lớn hơn 0.");
                }

                if (string.IsNullOrEmpty(StudentCode))
                {
                    throw new Exception("Mã khóa sinh là trường bắt buộc.");
                }


                // Kiểm tra xem StudentCode có tồn tại trong StudentCourse không
                var existingStudentCourse = await _unitOfWork.StudentCourse.GetByStudentCodeAsync(StudentCode);
                if (existingStudentCourse == null)
                {
                    throw new Exception("Mã khóa sinh không tồn tại.");
                }

                // Kiểm tra xem StudentCode có thuộc về CourseId này không
                if (existingStudentCourse.CourseId != CourseId)
                {
                    throw new Exception("Khóa sinh không tham gia khóa tu này.");
                }

                // Kiểm tra xem CourseId có tồn tại trong Course không
                var existingCourse = await _unitOfWork.Course.GetByIdAsync(CourseId);
                if (existingCourse == null)
                {
                    throw new InvalidOperationException("Khóa tu này không tồn tại");
                }


                // Lấy tất cả các StudentReport liên quan đến StudentId và CourseId
                var studentReports = await _unitOfWork.StudentReport.FindAsync(
                    sr => sr.StudentId == existingStudentCourse.StudentId && sr.Report.CourseId == CourseId && sr.Report.Status== ReportStatus.Read,
                    includeProperties: "Report"
                );

                // Ánh xạ các StudentReport thành StudentReportViewDto
                var reportViewDtos = studentReports.Select(sr => new StudentReportViewDto
                {
                    Date = sr.Report.ReportDate,
                    Status = sr.Status,
                    Content = sr.Comment ?? string.Empty,
                    ReportType = sr.Report.ReportType
                });

                return reportViewDtos;
            }
            else
            {
                var studentReports = await _unitOfWork.StudentReport.FindAsync(
                    sr => sr.StudentId == studentId && sr.Report.CourseId == CourseId,
                    includeProperties: "Report"
                );

                // Ánh xạ các StudentReport thành StudentReportViewDto
                var reportViewDtos = studentReports.Select(sr => new StudentReportViewDto
                {
                    Date = sr.Report.ReportDate,
                    Status = sr.Status,
                    Content = sr.Comment ?? string.Empty,
                    ReportType = sr.Report.ReportType
                });

                return reportViewDtos;
            }


        }
    }
}

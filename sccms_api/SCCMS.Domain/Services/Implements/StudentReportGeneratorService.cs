using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using System;
using System.Threading;
using System.Threading.Tasks;
using System.Linq;
using SCCMS.Domain.Services.Implements; // Điều chỉnh namespace phù hợp
using SCCMS.Infrastucture.Entities;
using Utility;
using SCCMS.Infrastucture.UnitOfWork; // Điều chỉnh namespace phù hợp
using System.Collections.Generic;
using System.Linq.Expressions;

namespace SCCMS.Domain.Services.Implements
{
    public class StudentReportGeneratorService : IHostedService, IDisposable
    {
        private Timer _timer;
        private readonly IServiceScopeFactory _scopeFactory;
        private readonly ILogger<StudentReportGeneratorService> _logger;

        public StudentReportGeneratorService(IServiceScopeFactory scopeFactory, ILogger<StudentReportGeneratorService> logger)
        {
            _scopeFactory = scopeFactory;
            _logger = logger;
        }

        public Task StartAsync(CancellationToken cancellationToken)
        {
            DoWork(null);
            // Tính toán thời gian đến lần chạy đầu tiên (00:00:00 UTC ngày tiếp theo)
            var now = DateTime.Now;
            var nextRunTime = DateTime.Now.Date.AddDays(1);
            var initialDelay = nextRunTime - now;

            if (initialDelay <= TimeSpan.Zero)
            {
                initialDelay = TimeSpan.FromDays(1);
            }

            // Đặt Timer để chạy vào đầu mỗi ngày UTC
            _timer = new Timer(DoWork, null, initialDelay, TimeSpan.FromDays(1));

            _logger.LogInformation("StudentReportGeneratorService đã được khởi động và sẽ chạy vào lúc {NextRunTime} UTC.", nextRunTime);

            return Task.CompletedTask;
        }

        private void DoWork(object state)
        {
            _logger.LogInformation("StudentReportGeneratorService đang thực hiện công việc tạo Reports và StudentReports vào lúc {Time}.", DateTime.Now);
            using (var scope = _scopeFactory.CreateScope())
            {
                var reportService = scope.ServiceProvider.GetRequiredService<IReportService>();
                var unitOfWork = scope.ServiceProvider.GetRequiredService<IUnitOfWork>();

                try
                {
                    GenerateReportsAndStudentReportsAsync(unitOfWork).GetAwaiter().GetResult();
                    _logger.LogInformation("Reports và StudentReports đã được tạo thành công vào lúc {Time}.", DateTime.Now);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Lỗi khi tạo Reports và StudentReports vào lúc {Time}.", DateTime.Now);
                }
            }
        }

        private async Task GenerateReportsAndStudentReportsAsync(IUnitOfWork unitOfWork)
        {
            var today = DateTime.Now.Date;

            // 1. Tạo Reports mới cho hôm nay
            await CreateDailyReportsAsync(unitOfWork, today);
            await CreateNightShiftReportsAsync(unitOfWork, today);

            // 2. Tạo StudentReports cho các Reports đã tạo
            await CreateStudentReportsAsync(unitOfWork, today);
        }

        private async Task CreateDailyReportsAsync(IUnitOfWork unitOfWork, DateTime today)
        {
            // Lấy tất cả các khóa tu đang hoạt động (status không closed hoặc deleted)
            var activeCourses = await unitOfWork.Course.FindAsync(
                c => c.StartDate.Date <= today && c.EndDate.Date >= today &&
                     c.Status != CourseStatus.closed && c.Status != CourseStatus.deleted,
                includeProperties: "StudentGroup"
            );

            var dailyReportsToAdd = new List<Report>();

            foreach (var course in activeCourses)
            {
                var studentGroups = course.StudentGroup ?? new List<StudentGroup>();

                foreach (var group in studentGroups)
                {
                    // Kiểm tra nếu DailyReport cho nhóm này đã tồn tại ngày hôm nay chưa
                    var existingReport = await unitOfWork.Report.GetAsync(
                        r => r.ReportDate.Date == today &&
                             r.ReportType == ReportType.DailyReport &&
                             r.StudentGroupId == group.Id
                    );

                    if (existingReport == null)
                    {
                        var dailyReport = new Report
                        {
                            CourseId = course.Id,
                            StudentGroupId = group.Id,
                            ReportDate = today,
                            ReportContent = $"Báo cáo hằng ngày chánh '{group.GroupName}', ngày {today:yyyy-MM-dd}",
                            ReportType = ReportType.DailyReport,
                            Status = ReportStatus.NotYet
                        };

                        dailyReportsToAdd.Add(dailyReport);
                    }
                }
            }

            if (dailyReportsToAdd.Any())
            {
                await unitOfWork.Report.AddRangeAsync(dailyReportsToAdd);
                _logger.LogInformation("{Count} DailyReports đã được tạo.", dailyReportsToAdd.Count);
            }

            await unitOfWork.SaveChangeAsync();
        }

        private async Task CreateNightShiftReportsAsync(IUnitOfWork unitOfWork, DateTime today)
        {
            // Lấy tất cả các NightShift đang hoạt động (status không closed hoặc deleted)
            var activeCourses = await unitOfWork.Course.FindAsync(
                c => c.StartDate.Date <= today && c.EndDate.Date >= today &&
                     c.Status != CourseStatus.closed && c.Status != CourseStatus.deleted,
                includeProperties: "NightShift"
            );

            var nightShiftReportsToAdd = new List<Report>();

            foreach (var course in activeCourses)
            {
                var nightShifts = course.NightShift ?? new List<NightShift>();

                foreach (var shift in nightShifts)
                {
                    // Lấy tất cả các Room liên quan đến NightShift này
                    var nightShiftRooms = await unitOfWork.Room.FindAsync(
                        r => r.CourseId == course.Id && r.NightShiftAssignment.Any(nsa => nsa.NightShiftId == shift.Id),
                        includeProperties: "NightShiftAssignment"
                    );

                    foreach (var room in nightShiftRooms)
                    {
                        // Kiểm tra nếu NightShiftReport cho NightShift và Room này đã tồn tại ngày hôm nay chưa
                        var existingReport = await unitOfWork.Report.GetAsync(
                            r => r.ReportDate.Date == today &&
                                 r.ReportType == ReportType.nightShift &&
                                 r.NightShiftId == shift.Id &&
                                 r.RoomId == room.Id
                        );

                        if (existingReport == null)
                        {
                            var nightShiftReport = new Report
                            {
                                CourseId = course.Id,
                                NightShiftId = shift.Id,
                                RoomId = room.Id,
                                ReportDate = today,
                                ReportContent = $"Báo cáo trực đêm cho phòng '{room.Name}', ngày {today:yyyy-MM-dd}",
                                ReportType = ReportType.nightShift,
                                Status = ReportStatus.NotYet
                            };

                            nightShiftReportsToAdd.Add(nightShiftReport);
                        }
                    }
                }
            }

            if (nightShiftReportsToAdd.Any())
            {
                await unitOfWork.Report.AddRangeAsync(nightShiftReportsToAdd);
                _logger.LogInformation("{Count} NightShiftReports đã được tạo.", nightShiftReportsToAdd.Count);
            }

            await unitOfWork.SaveChangeAsync();
        }

        private async Task CreateStudentReportsAsync(IUnitOfWork unitOfWork, DateTime today)
        {
            // Lấy tất cả các Reports cho ngày hôm nay
            var todayReports = await unitOfWork.Report.FindAsync(
                r => r.ReportDate.Date == today,
                includeProperties: "StudentGroup,Room,NightShift,StudentReports"
            );

            var newStudentReports = new List<StudentReport>();

            // Xử lý DailyReport
            var dailyReports = todayReports.Where(r => r.ReportType == ReportType.DailyReport && r.StudentGroupId.HasValue).ToList();

            if (dailyReports.Any())
            {
                // Lấy tất cả StudentGroupIds từ DailyReports
                var dailyGroupIds = dailyReports.Select(r => r.StudentGroupId.Value).Distinct().ToList();

                // Lấy tất cả các StudentGroupAssignment cho các nhóm này
                var dailyGroupAssignments = await unitOfWork.StudentGroupAssignment.FindAsync(
                    sga => dailyGroupIds.Contains(sga.StudentGroupId),
                    includeProperties: "Student"
                );

                // Tạo Dictionary để tra cứu sinh viên theo StudentGroupId
                var dailyGroupToStudents = dailyGroupAssignments
                    .GroupBy(sga => sga.StudentGroupId)
                    .ToDictionary(g => g.Key, g => g.Select(sga => sga.StudentId).ToList());

                foreach (var report in dailyReports)
                {
                    var groupId = report.StudentGroupId.Value;

                    if (dailyGroupToStudents.ContainsKey(groupId))
                    {
                        var studentIds = dailyGroupToStudents[groupId];

                        // Lấy tất cả StudentReport hiện tại cho báo cáo này
                        var existingStudentReports = await unitOfWork.StudentReport.FindAsync(
                            sr => sr.ReportId == report.Id,
                            includeProperties: "Student"
                        );

                        var existingStudentReportIds = existingStudentReports.Select(sr => sr.StudentId).ToHashSet();

                        // Tạo StudentReport mới nếu chưa tồn tại
                        foreach (var studentId in studentIds)
                        {
                            if (!existingStudentReportIds.Contains(studentId))
                            {
                                var newStudentReport = new StudentReport
                                {
                                    ReportId = report.Id,
                                    StudentId = studentId,
                                    Status = StudentReportStatus.Absent, // Hoặc trạng thái mặc định khác
                                    Comment = string.Empty // Hoặc giá trị mặc định khác
                                };

                                newStudentReports.Add(newStudentReport);
                            }
                        }
                    }
                }
            }

            // Xử lý NightShiftReport
            var nightShiftReports = todayReports.Where(r => r.ReportType == ReportType.nightShift && r.RoomId.HasValue).ToList();

            if (nightShiftReports.Any())
            {
                // Lấy tất cả RoomIds từ NightShiftReports
                var nightShiftRoomIds = nightShiftReports.Select(r => r.RoomId.Value).Distinct().ToList();

                // Lấy tất cả các StudentGroupAssignments trong các Room này
                // Giả sử mỗi Room có nhiều StudentGroup và mỗi StudentGroup có nhiều sinh viên
                var nightShiftStudentGroupAssignments = await unitOfWork.StudentGroupAssignment.FindAsync(
                    sga => nightShiftRoomIds.Contains(sga.StudentGroup.RoomId.Value),
                    includeProperties: "Student,StudentGroup.Room"
                );

                // Tạo Dictionary để tra cứu sinh viên theo RoomId
                var roomToStudents = nightShiftStudentGroupAssignments
                    .Where(sga => sga.StudentGroup.RoomId.HasValue)
                    .GroupBy(sga => sga.StudentGroup.RoomId.Value)
                    .ToDictionary(g => g.Key, g => g.Select(sga => sga.StudentId).Distinct().ToList());

                foreach (var report in nightShiftReports)
                {
                    var roomId = report.RoomId.Value;

                    if (roomToStudents.ContainsKey(roomId))
                    {
                        var studentIds = roomToStudents[roomId];

                        // Lấy tất cả StudentReport hiện tại cho báo cáo này
                        var existingStudentReports = await unitOfWork.StudentReport.FindAsync(
                            sr => sr.ReportId == report.Id,
                            includeProperties: "Student"
                        );

                        var existingStudentReportIds = existingStudentReports.Select(sr => sr.StudentId).ToHashSet();

                        // Tạo StudentReport mới nếu chưa tồn tại
                        foreach (var studentId in studentIds)
                        {
                            if (!existingStudentReportIds.Contains(studentId))
                            {
                                var newStudentReport = new StudentReport
                                {
                                    ReportId = report.Id,
                                    StudentId = studentId,
                                    Status = StudentReportStatus.Absent, // Hoặc trạng thái mặc định khác
                                    Comment = string.Empty // Hoặc giá trị mặc định khác
                                };

                                newStudentReports.Add(newStudentReport);
                            }
                        }
                    }
                }
            }

            // Thêm tất cả StudentReport mới vào cơ sở dữ liệu
            if (newStudentReports.Any())
            {
                await unitOfWork.StudentReport.AddRangeAsync(newStudentReports);
                _logger.LogInformation("{Count} StudentReports đã được tạo.", newStudentReports.Count);
            }

            // Lưu các thay đổi vào cơ sở dữ liệu
            await unitOfWork.SaveChangeAsync();
        }

        public Task StopAsync(CancellationToken cancellationToken)
        {
            _timer?.Change(Timeout.Infinite, 0);
            _logger.LogInformation("StudentReportGeneratorService đã dừng.");
            return Task.CompletedTask;
        }

        public void Dispose()
        {
            _timer?.Dispose();
        }
    }
}

using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using System;
using System.Threading;
using System.Threading.Tasks;
using System.Linq;
using SCCMS.Infrastucture.Entities;
using Utility;
using SCCMS.Infrastucture.UnitOfWork;

namespace SCCMS.Domain.Services.Implements
{
    public class CourseStatusUpdaterService : IHostedService, IDisposable
    {
        private Timer _timer;
        private readonly IServiceScopeFactory _scopeFactory;
        private readonly ILogger<CourseStatusUpdaterService> _logger;

        public CourseStatusUpdaterService(IServiceScopeFactory scopeFactory, ILogger<CourseStatusUpdaterService> logger)
        {
            _scopeFactory = scopeFactory;
            _logger = logger;
        }

        public Task StartAsync(CancellationToken cancellationToken)
        {
            // Thực hiện công việc ngay khi chương trình khởi động
            DoWork(null);

            // Tính toán thời gian đến lần chạy đầu tiên (00:00:00 ngày tiếp theo)
            var now = DateTime.Now;
            var nextRunTime = DateTime.Today.AddDays(1); // 00:00:00 ngày mai
            var timeToGo = nextRunTime - now;

            if (timeToGo <= TimeSpan.Zero)
            {
                timeToGo = TimeSpan.Zero; // Nếu đã qua 00:00:00, chạy ngay lập tức
            }

            // Đặt Timer chạy lần đầu sau timeToGo và lặp lại mỗi 24 giờ
            _timer = new Timer(DoWork, null, timeToGo, TimeSpan.FromDays(1));
            _logger.LogInformation("CourseStatusUpdaterService đã được khởi động và sẽ chạy vào lúc {NextRunTime}.", nextRunTime);

            return Task.CompletedTask;
        }

        private void DoWork(object state)
        {
            _logger.LogInformation("CourseStatusUpdaterService đang thực hiện công việc cập nhật trạng thái khóa tu vào lúc {Time}.", DateTime.Now);

            using (var scope = _scopeFactory.CreateScope())
            {
                var unitOfWork = scope.ServiceProvider.GetRequiredService<IUnitOfWork>();

                try
                {
                    UpdateCourseStatusesAsync(unitOfWork).GetAwaiter().GetResult();
                    _logger.LogInformation("Trạng thái khóa tu đã được cập nhật thành công vào lúc {Time}.", DateTime.Now);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Lỗi khi cập nhật trạng thái khóa tu vào lúc {Time}.", DateTime.Now);
                }
            }
        }

        private async Task UpdateCourseStatusesAsync(IUnitOfWork unitOfWork)
        {
            var today = DateTime.Today;
            var yesterday = today.AddDays(-1);

            // Lấy tất cả các khóa tu
            var courses = await unitOfWork.Course.GetAllAsync();

            foreach (var course in courses)
            {
                bool statusChanged = false;

                // Nếu ngày bắt đầu khóa tu là hôm nay
                if (course.StartDate.Date == today)
                {
                    course.Status = CourseStatus.inProgress;
                    statusChanged = true;
                }

                // Nếu ngày kết thúc khóa tu là ngày hôm qua
                if (course.EndDate.Date == yesterday)
                {
                    course.Status = CourseStatus.closed;
                    statusChanged = true;
                }

                if (statusChanged)
                {
                    await unitOfWork.Course.UpdateAsync(course);
                }
            }

            await unitOfWork.SaveChangeAsync();
        }

        public Task StopAsync(CancellationToken cancellationToken)
        {
            _timer?.Change(Timeout.Infinite, 0);
            _logger.LogInformation("CourseStatusUpdaterService đã dừng.");
            return Task.CompletedTask;
        }

        public void Dispose()
        {
            _timer?.Dispose();
        }
    }
}

using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SCCMS.Domain.Services.Implements
{
    public class ReportStatusUpdaterService : IHostedService, IDisposable
    {
        private Timer _timer;
        private readonly IServiceScopeFactory _scopeFactory;

        public ReportStatusUpdaterService(IServiceScopeFactory scopeFactory)
        {
            _scopeFactory = scopeFactory;
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
            _timer = new Timer(DoWork, null, timeToGo, TimeSpan.FromHours(24));
            return Task.CompletedTask;
        }

        private void DoWork(object state)
        {
            using (var scope = _scopeFactory.CreateScope())
            {
                var reportService = scope.ServiceProvider.GetRequiredService<IReportService>();
                reportService.UpdateReportStatusesAsync().Wait();
            }
        }

        public Task StopAsync(CancellationToken cancellationToken)
        {
            _timer?.Change(Timeout.Infinite, 0);
            return Task.CompletedTask;
        }

        public void Dispose()
        {
            _timer?.Dispose();
        }
    }

}
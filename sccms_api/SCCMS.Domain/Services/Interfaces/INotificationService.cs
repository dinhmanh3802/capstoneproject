using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SCCMS.Domain.Services.Interfaces
{
    public interface INotificationService
    {
        Task NotifyUserAsync(int userId, string message, string link);
        Task<IEnumerable<Infrastucture.Entities.Notification>> GetUserNotificationsAsync(int userId);
        Task MarkAsReadAsync(int notificationId);
        Task MarkAllAsReadAsync(int userId);
    }
}

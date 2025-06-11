using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;
using SCCMS.Domain.Hubs;
using SCCMS.Domain.Services.Interfaces;
using SCCMS.Infrastucture.Context;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SCCMS.Domain.Services.Implements
{
    public class NotificationService : INotificationService
    {
        private readonly AppDbContext _context;
        private readonly IHubContext<NotificationHub> _hubContext;

        public NotificationService(AppDbContext context, IHubContext<NotificationHub> hubContext)
        {
            _context = context;
            _hubContext = hubContext;
        }

        public async Task NotifyUserAsync(int userId, string message, string link)
        {
            var notification = new Infrastucture.Entities.Notification
            {
                UserId = userId,
                Message = message,
                Link = link,
                IsRead = false
            };
            _context.Notifications.Add(notification);
            await _context.SaveChangesAsync();

            await _hubContext.Clients.User(userId.ToString()).SendAsync("ReceiveNotification", message, link);
        }


        public async Task<IEnumerable<Infrastucture.Entities.Notification>> GetUserNotificationsAsync(int userId)
        {
            return await _context.Notifications
                .Where(n => n.UserId == userId)
                .OrderByDescending(n => n.CreatedAt)
                .ToListAsync();
        }

        public async Task MarkAsReadAsync(int notificationId)
        {
            var notification = await _context.Notifications.FindAsync(notificationId);
            if (notification != null)
            {
                notification.IsRead = true;
                await _context.SaveChangesAsync();
            }
        }
        public async Task MarkAllAsReadAsync(int userId)
        {
            var notifications = await _context.Notifications
                .Where(n => n.UserId == userId && !n.IsRead)
                .ToListAsync();

            if (notifications.Any())
            {
                foreach (var notification in notifications)
                {
                    notification.IsRead = true;
                }
                await _context.SaveChangesAsync();
            }
        }
    }
}

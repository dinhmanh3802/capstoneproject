using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using SCCMS.Domain.DTOs.NotificationDtos;
using SCCMS.Domain.Services.Interfaces;

namespace SCCMS.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class NotificationController : ControllerBase
    {
        private readonly INotificationService _notificationService;

        public NotificationController(INotificationService notificationService)
        {
            _notificationService = notificationService;
        }

        [HttpGet("{userId:int}")]
        public async Task<IEnumerable<Infrastucture.Entities.Notification>> GetUserNotifications(int userId)
        {
            return await _notificationService.GetUserNotificationsAsync(userId);
        }

        [HttpPost("mark-as-read/{notificationId:int}")]
        public async Task<IActionResult> MarkAsRead(int notificationId)
        {
            await _notificationService.MarkAsReadAsync(notificationId);
            return Ok();
        }
        [HttpPost("mark-all-as-read")]
        public async Task<IActionResult> MarkAllAsRead()
        {
            // Lấy userId từ token
            var userIdClaim = User.FindFirst("userId");
            if (userIdClaim == null || !int.TryParse(userIdClaim.Value, out int userId))
            {
                return Unauthorized();
            }

            await _notificationService.MarkAllAsReadAsync(userId);
            return Ok();
        }
    }
}

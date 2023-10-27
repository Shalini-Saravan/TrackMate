using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TrackMate.Api.Services;
using TrackMate.Models;

namespace TrackMate.Api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class NotificationController : ControllerBase
    {
        private readonly NotificationService NotificationService;

        public NotificationController(NotificationService notificationService)
        {
            NotificationService = notificationService;
        }

        [HttpGet("{userName}")]
        public Notification GetNotificationsByUserName(string userName)
        {
            return NotificationService.GetNotificationsByUserName(userName);
        }

        [HttpGet("{userName}/count")]
        public int GetNotificationsCount(string userName)
        {
            return NotificationService.GetNotificationsCount(userName);
        }

        [HttpDelete("{userName}/clear/{id}")]
        public async void ClearNotificationById(string userName, string id)
        {
            await NotificationService.ClearNotificationById(userName, id);
        }
        [HttpDelete("{userName}/clearAll")]
        public async void ClearAllNotificationByUserName(string userName)
        {
            await NotificationService.ClearAllNotificationByUserName(userName);
        }
    }

}

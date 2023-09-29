using Blazored.LocalStorage;
using BlazorServerAppWithIdentity.Models;
using Microsoft.AspNetCore.SignalR;
using System.Net.Http.Headers;

namespace BlazorServerAppWithIdentity.Services
{
    public class NotificationService
    {
        private readonly HttpClient httpClient;
        private readonly ILocalStorageService LocalStorageService;
        public NotificationService(ILocalStorageService LocalStorageService, HttpClient httpClient)
        {
            this.httpClient = httpClient;
            this.LocalStorageService = LocalStorageService;
        }
        public int GetNotificationsCount(string userName)
        { 
            if(userName == null || userName == string.Empty) return 0;
            return httpClient?.GetFromJsonAsync<int>("api/notification/" + userName + "/count").Result ?? 0;
        }
        public Notification GetNotificationsByUserName(string userName) 
        {
            if (userName == null || userName == string.Empty) return new Notification();
            return httpClient?.GetFromJsonAsync<Notification>("api/notification/" + userName).Result ?? new Notification();
        }

        public void ClearNotificationById(string userName, int id)
        {
            if (userName == null || userName == string.Empty) return;
            httpClient?.DeleteAsync("api/notification/" + userName + "/clear/" + id);
        }
        public void ClearAllNotificationByUserName(string userName)
        {
            if (userName == null || userName == string.Empty) return;
            httpClient?.DeleteAsync("api/notification/" + userName + "/clearAll");
        }
    }
}

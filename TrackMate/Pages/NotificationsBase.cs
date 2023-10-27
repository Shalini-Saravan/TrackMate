using Blazored.LocalStorage;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.SignalR.Client;
using System.Configuration;
using System.Net.Mail;
using TrackMate.Models;
using TrackMate.Services;

namespace TrackMate.Pages
{
    public class NotificationsBase : ComponentBase, IAsyncDisposable
    {
        [Inject]
        public NotificationService NotificationService { get; set; }
        [Inject]
        public IHttpContextAccessor? HttpContextAccessor { get; set; }

        [Inject]
        public IConfiguration? configuration { get; set; }
        [Inject]
        public SubscriptionService SubscriptionService { get; set; }
        private HubConnection hubConnection { get; set; }
        protected Notification NotificationsList { get; set; }
        protected Subscription subscription { get; set; } = new Subscription();
        protected bool isSubmitting { get; set; }
        protected string displayButton { get; set; } = "none";
        [Inject]
        protected ILocalStorageService LocalStorageService { get; set; }

        protected async override void OnInitialized()
        {
            base.OnInitialized();
            try
            {
                NotificationsList = NotificationService.GetNotificationsByUserName(HttpContextAccessor.HttpContext?.User?.Identity.Name);
                subscription = SubscriptionService.GetSubscriptionByUserName(HttpContextAccessor.HttpContext?.User?.Identity.Name);
            }
            catch (Exception)
            {
                NotificationsList = new Notification();
            }
            if (configuration != null)
            {
                string TokenValue = await LocalStorageService.GetItemAsStringAsync("TokenValue");
                hubConnection = new HubConnectionBuilder()
                   .WithUrl(configuration["HubUrl"], options =>
                   {
                       options.AccessTokenProvider = () => Task.FromResult(TokenValue ?? null);
                   })
                   .Build();

                hubConnection.On<string>("privateNotification", OnNotificationReceived);
                hubConnection.On<string>("privateNotification-clear", OnNotificationCleared);
                hubConnection.StartAsync();
            }
        }
        public void UpdateSubscription()
        {
            if (isSubmitting) return;
            isSubmitting = true;
            try
            {
                SubscriptionService.UpdateSubscription(subscription, HttpContextAccessor.HttpContext?.User?.Identity.Name);
            }
            finally { isSubmitting = false; }

        }

        public void OnNotificationReceived(string userName)
        {
            try
            {
                NotificationsList = NotificationService.GetNotificationsByUserName(userName);
            }
            catch (Exception)
            {
                NotificationsList = new Notification();
            }
            StateHasChanged();
        }
        public void OnNotificationCleared(string userName)
        {
            try
            {
                NotificationsList = NotificationService.GetNotificationsByUserName(userName);
            }
            catch (Exception)
            {
                NotificationsList = new Notification();
            }
            StateHasChanged();
        }
        public void clearAllNotification()
        {
            NotificationService.ClearAllNotificationByUserName(HttpContextAccessor.HttpContext?.User?.Identity.Name);
        }
        public void clearNotification(int id)
        {
            NotificationService.ClearNotificationById(HttpContextAccessor.HttpContext?.User?.Identity.Name, id);
        }
       
        public async ValueTask DisposeAsync()
        {
            if (hubConnection != null)
            {
                await hubConnection.DisposeAsync();
            }
        }
    }
}

using BlazorServerAppWithIdentity.Api.Data;
using BlazorServerAppWithIdentity.Api.Hubs;
using BlazorServerAppWithIdentity.Models;
using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.Options;
using MongoDB.Driver;
using System.Reflection.PortableExecutable;
using System.Security.Claims;

namespace BlazorServerAppWithIdentity.Services
{
    public class NotificationService
    {
        private readonly BlazorServerAppWithIdentityContext db;
        public IHubContext<BlazorHub> _hubContext { get; set; }

        public NotificationService(IMongoClient client, IHubContext<BlazorHub> _hubContext)
        {
            db = new BlazorServerAppWithIdentityContext(client);
            this._hubContext = _hubContext;
        }
        public Notification GetNotificationsByUserName(string userName)
        {
            return db.Notifications.Find(o => o.UserName == userName).FirstOrDefault() ?? new Notification();
        }
        public int GetNotificationsCount(string userName)
        {
            return db.Notifications.Find(g => g.UserName == userName).FirstOrDefault()?.Messages.Count() ?? 0;
        }
        public async Task ClearNotificationById(string userName, string id)
        {
            try
            {
                Notification notification = db.Notifications.Find(o => o.UserName == userName).FirstOrDefault();
                if (notification != null)
                {
                    notification.Messages.RemoveAt(int.Parse(id));
                    await db.Notifications.ReplaceOneAsync(filter: g => g.UserName == userName, replacement: notification);
                    await _hubContext.Clients.User(userName.ToLower()).SendAsync("privateNotification-clear", userName);
                }
              
            }
            catch
            {
                System.Diagnostics.Debug.WriteLine("Exception Occurred - Clear Notification");
            }
        }
        public async Task ClearAllNotificationByUserName(string userName)
        {
            try
            {
                Notification notification = db.Notifications.Find(o => o.UserName == userName).FirstOrDefault();
                if (notification != null)
                {
                    notification.Messages.Clear();
                    await db.Notifications.ReplaceOneAsync(filter: g => g.UserName == userName, replacement: notification);
                    await _hubContext.Clients.User(userName.ToLower()).SendAsync("privateNotification-clear", userName);
                }

            }
            catch
            {
                System.Diagnostics.Debug.WriteLine("Exception Occurred - Clear Notification");
            }
        }
        public async Task SendPrivateNotification(string userName, string messageBody, string subscription)
        {
            //Adding notification to the user record
            bool subscribed = true;
            if(subscription != "Default")
            {
                Subscription subscribe = db.Subscription.Find(o => o.UserName == userName).FirstOrDefault();
                if(subscription == "Machine_CheckIns_CheckOuts" &&  subscribe != null && !subscribe.Machine_CheckIns_CheckOuts)
                {
                    subscribed = false;
                }
                
            }
            if (subscribed)
            {
                Notification notificationRec = db.Notifications.Find(Builders<Notification>.Filter.Eq("UserName", userName)).FirstOrDefault();
                if (notificationRec != null)
                {
                    notificationRec.Messages.Add(new Message
                    {
                        Body = messageBody,
                        TimeStamp = DateTime.UtcNow.AddMinutes(330)
                    });
                    var Notificationupdate = Builders<Notification>.Update
                        .Set(m => m.Messages, notificationRec.Messages);

                    await db.Notifications.UpdateOneAsync(filter: g => g.UserName == userName, Notificationupdate);
                }
                else
                {
                    await db.Notifications.InsertOneAsync(new Notification
                    {
                        UserName = userName,
                        Messages = new List<Message>{
                                            new Message {
                                                Body = messageBody,
                                                TimeStamp = DateTime.UtcNow.AddMinutes(330)
                                            }
                                        }
                    });
                }
                await _hubContext.Clients.User(userName.ToLower()).SendAsync("privateNotification", userName);
            }
        }

    }
}

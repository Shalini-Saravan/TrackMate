﻿using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.Options;
using MongoDB.Driver;
using System.Reflection.PortableExecutable;
using System.Security.Claims;
using TrackMate.Api.Data;
using TrackMate.Api.Hubs;
using TrackMate.Models;

namespace TrackMate.Api.Services
{
    public class SubscriptionService
    {
        private readonly DbContext db;
        public IHubContext<BlazorHub> _hubContext { get; set; }
        public NotificationService NotificationService { get; set; }

        public SubscriptionService(IMongoClient client, IHubContext<BlazorHub> _hubContext, NotificationService notificationService)
        {
            db = new DbContext(client);
            this._hubContext = _hubContext;
            NotificationService = notificationService;
        }
        public Subscription GetSubscriptionByUserName(string userName)
        {
            return db.Subscription.FindAsync(o => o.UserName == userName).Result.FirstOrDefault();
        }
        public async Task<string> UpdateSubscription(Subscription? subscription, string userName)
        {
            try
            {
                if (subscription != null)
                {
                    subscription.UserName = userName;
                    Subscription existingSubscription = GetSubscriptionByUserName(userName);
                    if (existingSubscription != null)
                    {
                        await db.Subscription.ReplaceOneAsync(filter: g => g.UserName == userName, replacement: subscription);
                    }
                    else
                    {
                        await db.Subscription.InsertOneAsync(subscription);
                    }
                    await NotificationService.SendPrivateNotification(userName, "Your Subscription Policy has been updated.", "Default");
                    return "Subscription Updated Successfully!";
                }
                else
                {
                    return "Failed Operation!";
                }
            }
            catch
            {
                return "Error: Subscription Update Failed";
            }
        }

    }
}

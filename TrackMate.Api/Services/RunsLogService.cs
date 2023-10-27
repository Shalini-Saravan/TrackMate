using Microsoft.AspNetCore.SignalR;
using MongoDB.Driver;
using TrackMate.Api.Data;
using TrackMate.Models;

namespace TrackMate.Api.Services
{
    public class RunsLogService
    {
        private readonly DbContext db;
        private readonly SubscriptionService SubscriptionService;
        public RunsLogService(IMongoClient client, SubscriptionService SubscriptionService)
        {
            db = new DbContext(client);
            this.SubscriptionService = SubscriptionService;
        }

        public string AddRunsLog(RunsLog log)
        {
            try
            {
                Subscription subscription = SubscriptionService.GetSubscriptionByUserName(log.UserName);
                if (subscription != null && subscription.Pipeline_Build_Completion)
                {
                    var response = db.RunsLog.InsertOneAsync(log);
                    return "Log Added Successfully!";
                }
                else
                {
                    return "Not Subscribed!";
                }
            }
            catch
            {
                return "Error: Failed to add new log";
            }
        }
    }
}

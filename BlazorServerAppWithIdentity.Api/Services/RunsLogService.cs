using BlazorServerAppWithIdentity.Api.Data;
using BlazorServerAppWithIdentity.Models;
using BlazorServerAppWithIdentity.Services;
using Microsoft.AspNetCore.SignalR;
using MongoDB.Driver;

namespace BlazorServerAppWithIdentity.Api.Services
{
    public class RunsLogService
    {
        private readonly BlazorServerAppWithIdentityContext db;
        private readonly SubscriptionService SubscriptionService;
        public RunsLogService(IMongoClient client, SubscriptionService SubscriptionService)
        {
            db = new BlazorServerAppWithIdentityContext(client);
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

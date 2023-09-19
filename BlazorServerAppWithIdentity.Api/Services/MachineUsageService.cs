using BlazorServerAppWithIdentity.Api.Data;
using BlazorServerAppWithIdentity.Api.Hubs;
using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.Options;
using MongoDB.Driver;
using System.Security.Claims;

namespace BlazorServerAppWithIdentity.Services
{
    public class MachineUsageService
    {
        private readonly BlazorServerAppWithIdentityContext db;

        public MachineUsageService(IMongoClient client)
        {
            db = new BlazorServerAppWithIdentityContext(client);
        }
        public IEnumerable<Models.MachineUsage> GetMachineUsageByUserName(string userName)
        {
            return db.MachineUsage.Find(g => g.UserName == userName).SortByDescending(g => g.StartTime).Limit(10).ToList();
        }

    }
}

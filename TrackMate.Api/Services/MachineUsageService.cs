using TrackMate.Api.Hubs;
using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.Options;
using MongoDB.Driver;
using System.Security.Claims;
using TrackMate.Api.Data;
using TrackMate.Models;

namespace TrackMate.Api.Services
{
    public class MachineUsageService
    {
        private readonly DbContext db;

        public MachineUsageService(IMongoClient client)
        {
            db = new DbContext(client);
        }
        public IEnumerable<MachineUsage> GetMachineUsageByUserName(string userName)
        {
            return db.MachineUsage.Find(g => g.UserName == userName).SortByDescending(g => g.StartTime).Limit(10).ToList();
        }

    }
}

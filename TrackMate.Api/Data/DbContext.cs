using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using MongoDB.Bson;
using MongoDB.Driver;
using MongoDB.Driver.Core.Configuration;
using System.Xml.Linq;
using TrackMate.Models;

namespace TrackMate.Api.Data
{
    public class DbContext : IdentityDbContext
    {
        private readonly IMongoDatabase _mongoDatabase;


        public DbContext(IMongoClient _client)
        {
            _mongoDatabase = _client.GetDatabase("IdentityAuthDb");
        }

        public IMongoDatabase GetMongoDatabase() { return _mongoDatabase; }



        public IMongoCollection<Machine> MachineRecords
        {
            get
            {
                return _mongoDatabase.GetCollection<Machine>("MachineRecords");
            }
        }

        public IMongoCollection<MachineUsage> MachineUsage
        {
            get
            {
                return _mongoDatabase.GetCollection<MachineUsage>("MachineUsage");
            }
        }
        public IMongoCollection<Notification> Notifications
        {
            get
            {
                return _mongoDatabase.GetCollection<Notification>("Notifications");
            }
        }
        public IMongoCollection<Subscription> Subscription
        {
            get
            {
                return _mongoDatabase.GetCollection<Subscription>("Subscription");
            }
        }
        public IMongoCollection<RunsLog> RunsLog
        {
            get
            {
                return _mongoDatabase.GetCollection<RunsLog>("RunsLog");
            }
        }
    }
}

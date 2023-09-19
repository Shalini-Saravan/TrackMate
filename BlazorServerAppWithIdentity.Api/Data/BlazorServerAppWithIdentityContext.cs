using BlazorServerAppWithIdentity.Models;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using MongoDB.Bson;
using MongoDB.Driver;
using MongoDB.Driver.Core.Configuration;
using System.Xml.Linq;

namespace BlazorServerAppWithIdentity.Api.Data
{
    public class BlazorServerAppWithIdentityContext : IdentityDbContext
    {
        private readonly IMongoDatabase _mongoDatabase;


        public BlazorServerAppWithIdentityContext(IMongoClient _client)
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


    }
}

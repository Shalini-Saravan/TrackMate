using BlazorServerAppWithIdentity.Api.Hubs;
using BlazorServerAppWithIdentity.Models;
using Microsoft.AspNetCore.SignalR;
using MongoDB.Driver;
using Newtonsoft.Json.Linq;
using System.Reflection.PortableExecutable;
using System.Text;
using Machine = BlazorServerAppWithIdentity.Models.Machine;

namespace BlazorServerAppWithIdentity.Api.Services.BackgroundServices
{
    public class MachineTimeout : BackgroundService
    {
        private Timer _timer;
        private IMongoDatabase MongoDatabase { get; set; }
        private IMongoCollection<Machine> MachineRecords { get; set; }
        private IMongoCollection<MachineUsage> MachineUsageRecords { get; set; }
        private IMongoCollection<Notification> NotificationRecords { get; set; }
        private IHubContext<BlazorHub> _hubContext { get; set; }
        public MachineTimeout(IHubContext<BlazorHub> HubContext)
        {
            this._hubContext = HubContext;
        }
        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            var settings = MongoClientSettings.FromConnectionString("mongodb+srv://shalini:T3874ve7Dt5nfsOd@cluster0.isgqskk.mongodb.net/?retryWrites=true&w=majority");
            settings.ServerApi = new ServerApi(ServerApiVersion.V1);
            var client = new MongoClient(settings);
            MongoDatabase = client.GetDatabase("IdentityAuthDb");
            MachineRecords = MongoDatabase.GetCollection<Machine>("MachineRecords");
            MachineUsageRecords = MongoDatabase.GetCollection<MachineUsage>("MachineUsage");
            NotificationRecords = MongoDatabase.GetCollection<Notification>("Notifications");

            await Task.Delay(1000);
            _timer = new Timer(UpdateTimeoutMachine, null, TimeSpan.Zero, TimeSpan.FromHours(2));
        }
        protected void UpdateTimeoutMachine(object? state)
        {
            foreach (var record in MachineRecords.Find(m => m.Status == "Not Available").ToList())
            {
                if (record.EndTime < DateTime.UtcNow.AddMinutes(330) || (record.EndTime < DateTime.UtcNow.AddMinutes(330).AddDays(1) && record.EndTime?.AddMinutes(121) > DateTime.UtcNow.AddDays(1).AddMinutes(330) ))
                {
                    try
                    {
                        var logupdate = Builders<Models.MachineUsage>.Update
                        .Set(m => m.EndTime, DateTime.UtcNow.AddMinutes(330))
                        .Set(m => m.InUse, false);
                        var filter1 = Builders<MachineUsage>.Filter.Eq("InUse", true);
                        var filter2 = Builders<MachineUsage>.Filter.Eq("Machine.Id", record.Id);
                        var filter3 = Builders<MachineUsage>.Filter.Eq("UserName", record.UserName);
                        MachineUsageRecords?.UpdateOneAsync(filter: filter1 & filter2 & filter3, logupdate);
                        
                        string userName = record.UserName;
                        var update = Builders<Models.Machine>.Update
                        .Set(m => m.Status, "Available").Set(m => m.UserId, null)
                        .Set(m => m.Comments, "None")
                        .Set(m => m.UserName, null)
                        .Set(m => m.UserId, null);
                        MachineRecords.UpdateOneAsync(filter: g => g.Id == record.Id, update);

                        //Adding notification to the user record
                        Notification notificationRec = NotificationRecords.Find(Builders<Notification>.Filter.Eq("UserName", userName)).FirstOrDefault();
                        if(notificationRec != null)
                        {
                            if (record.EndTime < DateTime.UtcNow.AddMinutes(330))
                            {
                                notificationRec.Messages.Add(new Message
                                {
                                    Body = "Machine" + record.Name + " has been Timedout!",
                                    TimeStamp = DateTime.UtcNow.AddMinutes(330)
                                });
                            }
                            else
                            {
                                notificationRec.Messages.Add(new Message
                                {
                                    Body = "Remainder: Reservation for the Machine " + record.Name + " will be expired in a day!",
                                    TimeStamp = DateTime.UtcNow.AddMinutes(330)
                                });
                            }
                            var Notificationupdate = Builders<Notification>.Update
                                .Set(m => m.Messages,notificationRec.Messages);

                            NotificationRecords.UpdateOneAsync(filter: g => g.UserName == userName, Notificationupdate);
                        }
                        else
                        {
                            NotificationRecords.InsertOneAsync(new Notification
                            {
                                UserName = userName,
                                Messages = new List<Message>{
                                            new Message {
                                                Body = "Machine has been Timedout!",
                                                TimeStamp = DateTime.UtcNow.AddMinutes(330)
                                            }
                                        }
                            });
                        }
                        _hubContext.Clients.User(userName).SendAsync("privateNotification",
                        userName);
                    }
                    catch (Exception ex)
                    {
                        System.Diagnostics.Debug.WriteLine("Exception occurred : " + ex);
                    }
                }
            }
        }

    }
}

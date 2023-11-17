using Microsoft.AspNetCore.SignalR;
using MongoDB.Driver;
using Newtonsoft.Json.Linq;
using System.Net.Mail;
using System.Reflection.PortableExecutable;
using System.Text;
using TrackMate.Api.Hubs;
using TrackMate.Models;
using Machine = TrackMate.Models.Machine;

namespace TrackMate.Api.Services.BackgroundServices
{
    public class MachineTimeout : BackgroundService
    {
        private Timer _timer;
        private IMongoDatabase MongoDatabase { get; set; }
        private IMongoCollection<Machine> MachineRecords { get; set; }
        private IMongoCollection<MachineUsage> MachineUsageRecords { get; set; }
        private IMongoCollection<Notification> NotificationRecords { get; set; }
        private IMongoCollection<Subscription> SubscriptionRecords { get; set; }
        private IHubContext<BlazorHub> _hubContext { get; set; }
        public MachineTimeout(IHubContext<BlazorHub> HubContext)
        {
            _hubContext = HubContext;
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
            SubscriptionRecords = MongoDatabase.GetCollection<Subscription>("Subscription");

            await Task.Delay(1000);
            _timer = new Timer(UpdateTimeoutMachine, null, TimeSpan.Zero, TimeSpan.FromHours(2));
        }

        protected void UpdateTimeoutMachine(object? state)
        {
            foreach (var record in MachineRecords.Find(m => m.Status == "Not Available").ToList())
            {
                string userName = record.UserName;

                //machine expired
                if (record.EndTime < DateTime.UtcNow.AddMinutes(330))
                {
                    try
                    {

                        var logupdate = Builders<MachineUsage>.Update
                            .Set(m => m.EndTime, DateTime.UtcNow.AddMinutes(330))
                            .Set(m => m.InUse, false);
                        var filter1 = Builders<MachineUsage>.Filter.Eq("InUse", true);
                        var filter2 = Builders<MachineUsage>.Filter.Eq("Machine.Id", record.Id);
                        var filter3 = Builders<MachineUsage>.Filter.Eq("UserName", record.UserName);
                        MachineUsageRecords?.UpdateOneAsync(filter: filter1 & filter2 & filter3, logupdate);

                        var update = Builders<Machine>.Update
                            .Set(m => m.Status, "Available").Set(m => m.UserId, null)
                            .Set(m => m.Comments, "None")
                            .Set(m => m.UserName, null)
                            .Set(m => m.UserId, null);
                        MachineRecords.UpdateOneAsync(filter: g => g.Id == record.Id, update);

                        SendPrivateNotification(userName, "Machine " + record.Name + " has been Timedout!");

                    }
                    catch (Exception ex)
                    {
                        System.Diagnostics.Debug.WriteLine("Exception occurred : " + ex);
                    }
                }
                else if (record.EndTime < DateTime.UtcNow.AddMinutes(330).AddDays(1) && record.EndTime?.AddMinutes(121) > DateTime.UtcNow.AddDays(1).AddMinutes(330))
                {
                    try
                    {
                        SendPrivateNotification(userName, "Remainder: Reservation for the Machine " + record.Name + " will be expired in a day!");
                    }
                    catch { }
                }

            }

        }
        public void SendEmailNotification(string toMail, string subject, string body)
        {
            try
            {
                using (MailMessage mail = new MailMessage())
                {
                    mail.From = new MailAddress("shalini.saravanan@ni.com");
                    mail.To.Add(toMail);
                    mail.Subject = subject;
                    mail.IsBodyHtml = true;
                    mail.Body = body;

                    using (SmtpClient smtp = new SmtpClient("mailout.natinst.com", 25))
                    {
                        smtp.Credentials = new System.Net.NetworkCredential("shalini.saravanan@ni.com", "");
                        smtp.EnableSsl = true;
                        smtp.Send(mail);
                    }
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine("Error " + ex.ToString());
            }
        }

        public void SendPrivateNotification(string userName, string messageBody)
        {
            //Adding notification to the user record
            bool subscribed = true;

            Subscription subscribe = SubscriptionRecords.Find(o => o.UserName == userName).FirstOrDefault();
            if (subscribe != null && !subscribe.Machine_CheckIns_CheckOuts)
            {
                subscribed = false;
            }
            if (subscribe != null && subscribe.Email_TimeOut_Notification)
            {
                string body = "Hi " + userName.Substring(0, userName.IndexOf(".")) + ", <br /> " + messageBody + "<br /><br/ > Thanks, <br /> TrackMate.";
                SendEmailNotification(userName + "@ni.com", "TrackMate - Machine Status", body);
            }

            if (subscribed)
            {
                Notification notificationRec = NotificationRecords.Find(Builders<Notification>.Filter.Eq("UserName", userName)).FirstOrDefault();
                if (notificationRec != null)
                {
                    notificationRec.Messages.Add(new Message
                    {
                        Body = messageBody,
                        TimeStamp = DateTime.UtcNow.AddMinutes(330)
                    });
                    var Notificationupdate = Builders<Notification>.Update
                        .Set(m => m.Messages, notificationRec.Messages);

                    NotificationRecords.UpdateOneAsync(filter: g => g.UserName == userName, Notificationupdate);
                }
                else
                {
                    NotificationRecords.InsertOneAsync(new Notification
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
                _hubContext.Clients.User(userName.ToLower()).SendAsync("privateNotification", userName);
            }
        }

    }
}

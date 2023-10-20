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
    public class PipelineNotification : BackgroundService
    {
        private Timer _timer;
        private IMongoDatabase MongoDatabase { get; set; }
        private IMongoCollection<RunsLog> RunsLogRecords { get; set; }
        private IMongoCollection<Notification> NotificationRecords { get; set; }
        private IHubContext<BlazorHub> _hubContext { get; set; }
        private HttpClient httpClient { get; set; }
        private string PAT { get; set; } = "";

        public PipelineNotification(IHubContext<BlazorHub> HubContext)
        {
            this._hubContext = HubContext;
        }
        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            var settings = MongoClientSettings.FromConnectionString("mongodb+srv://shalini:T3874ve7Dt5nfsOd@cluster0.isgqskk.mongodb.net/?retryWrites=true&w=majority");
            settings.ServerApi = new ServerApi(ServerApiVersion.V1);
            var client = new MongoClient(settings);
            MongoDatabase = client.GetDatabase("IdentityAuthDb");
            RunsLogRecords = MongoDatabase.GetCollection<RunsLog>("RunsLog");
            NotificationRecords = MongoDatabase.GetCollection<Notification>("Notifications");

            httpClient = new HttpClient();
            httpClient.BaseAddress = new Uri("http://api:443");
            httpClient.DefaultRequestHeaders.Add("Authorization", "Basic " + Convert.ToBase64String(ASCIIEncoding.ASCII.GetBytes(":" + PAT)));
            httpClient.DefaultRequestHeaders.Accept.Add(new System.Net.Http.Headers.MediaTypeWithQualityHeaderValue("application/json"));
           
            await Task.Delay(1000);
            _timer = new Timer(UpdatePipelineRuns, null, TimeSpan.Zero, TimeSpan.FromMinutes(30));
        }
        protected void UpdatePipelineRuns(object? state)
        {
            foreach (var rec in RunsLogRecords.Find(_ => true).ToList())
            {
                var resp = httpClient.GetAsync("https://dev.azure.com/ni/devcentral/_apis/pipelines/" + rec.PipelineId + "/runs/" + rec.RunId + "?api-version=7.1-preview.1").Result;
                JObject response = JObject.Parse(resp.Content.ReadAsStringAsync().Result);
                string responseBody = response.GetValue("state")?.ToString() ?? "";
                if(responseBody == "completed")
                {
                    try
                    {
                        RunsLog record = rec;
                        RunsLogRecords.DeleteOneAsync(filter: o => o.Id == record.Id);
                        //Adding notification to the user record
                        Notification notificationRec = NotificationRecords.Find(Builders<Notification>.Filter.Eq("UserName", record.UserName)).FirstOrDefault();
                        if (notificationRec != null)
                        {
                            notificationRec.Messages.Add(new Message
                            {
                                Body = "Build " + record.RunId + " has been " + (response.GetValue("result") ?? "completed"),
                                TimeStamp = DateTime.UtcNow.AddMinutes(330)
                            });
                            var Notificationupdate = Builders<Notification>.Update
                                .Set(m => m.Messages, notificationRec.Messages);

                            NotificationRecords.UpdateOneAsync(filter: g => g.UserName == record.UserName, Notificationupdate);
                        }
                        else
                        {
                            NotificationRecords.InsertOneAsync(new Notification
                            {
                                UserName = record.UserName,
                                Messages = new List<Message>{
                                            new Message {
                                                Body = "Build " + record.RunId + " has been " + (response.GetValue("result")?? "completed"),
                                                TimeStamp = DateTime.UtcNow.AddMinutes(330)
                                            }
                                        }
                            });
                        }
                        _hubContext.Clients.User(record.UserName).SendAsync("privateNotification",
                        record.UserName);
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

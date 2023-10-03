using BlazorServerAppWithIdentity.Models;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace BlazorServerAppWithIdentity.Services
{
    public class RunsLogService
    {
        private readonly HttpClient httpClient;
        public RunsLogService(HttpClient httpClient)
        {
            this.httpClient = httpClient;
        }

        public void AddRunsLog(RunsLog log)
        {
            var jsonString = "{'runsLog' :" + JsonConvert.SerializeObject(log) + "}";
            var response = httpClient.PostAsJsonAsync("api/runslog", jsonString).Result;
        }
    }
}

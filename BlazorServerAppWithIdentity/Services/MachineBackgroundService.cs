using BlazorServerAppWithIdentity.Models;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.Net.Http;
using System.Text;

namespace BlazorServerAppWithIdentity.Services
{
    public class MachineBackgroundService : BackgroundService
    {
        private Timer _timer;
        private HttpClient httpClient { get; set; }
        
        private string PAT { get; set; } = "uglgunvohxdewpe2swfxtvf6ho2zk7feg6xdsufsimnomyengaca";

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            httpClient = new HttpClient();
            httpClient.BaseAddress = new Uri("https://localhost:7035/");

            httpClient.DefaultRequestHeaders.Add("Authorization", "Basic " + Convert.ToBase64String(ASCIIEncoding.ASCII.GetBytes(":" + PAT)));
            httpClient.DefaultRequestHeaders.Accept.Add( new System.Net.Http.Headers.MediaTypeWithQualityHeaderValue("application/json"));
            await Task.Delay(1000);
            _timer = new Timer(ReloadAgents, null, TimeSpan.Zero, TimeSpan.FromSeconds(60));
        }

        private void ReloadAgents(object? state)
        {
            try
            {
                List<Machine>? agentsDB = httpClient.GetFromJsonAsync<List<Machine>>("api/machine/agents").Result.OrderBy(o => o.AgentId).ToList();

                var resp = httpClient.GetAsync("https://dev.azure.com/ni/_apis/distributedtask/pools/426/agents?api-version=7.1-preview.1").Result;
                JObject response = JObject.Parse(resp.Content.ReadAsStringAsync().Result);
                string responseBody = response.GetValue("value")?.ToString() ?? "";
                List<Agent> agentsAPI = JsonConvert.DeserializeObject<List<Agent>>(responseBody);
                agentsAPI = agentsAPI.OrderBy(o => o.Id).ToList();
                
                int i = 0, j = 0;
                while (i < agentsAPI.Count && j < agentsDB.Count)
                {
                    string agentId = agentsDB[j].AgentId;
                    if (agentId != null)
                    {
                        if (agentsAPI[i].Id < int.Parse(agentId))
                        {
                            //insert at j
                            Machine machine = new Machine();
                            machine.AgentId = agentsAPI[i].Id.ToString();
                            machine.Name = agentsAPI[i].Name;
                            machine.Status = "Available";
                            machine.Type = "Virtual";
                            machine.Comments = "None";
                            machine.LastAccessed = DateTime.UtcNow;
                            if (string.Compare(machine.Name, "TS-TEST00") == 1 && string.Compare(machine.Name, "TS-TEST21") == -1)
                            {
                                machine.Purpose = "Automated";
                            }
                            else
                            {
                                machine.Purpose = "Manual";
                            }
                            var jsonString = "{'machine' :" + JsonConvert.SerializeObject(machine) + "}";
                            var httpresponse = httpClient.PostAsJsonAsync("api/machine", jsonString).Result;

                            i++;
                        }
                        else if (agentsAPI[i].Id > int.Parse(agentId))
                        {
                            //delete at j
                            httpClient.DeleteAsync("api/machine/" + agentsDB[j].Id);
                            j++;
                        }
                        else
                        {
                            i++;
                            j++;
                        }
                    }
                }

                while (i < agentsAPI.Count)
                {
                    Machine machine = new Machine();
                    machine.AgentId = agentsAPI[i].Id.ToString();
                    machine.Status = "Available";
                    machine.Type = "Virtual";
                    machine.Name = agentsAPI[i].Name;
                    machine.Comments = "None";
                    machine.LastAccessed = DateTime.UtcNow;
                    if (string.Compare(machine.Name, "TS-TEST00") == 1 && string.Compare(machine.Name, "TS-TEST21") == -1)
                    {
                        machine.Purpose = "Automated";
                    }
                    else
                    {
                        machine.Purpose = "Manual";
                    }
                    var jsonString = "{'machine' :" + JsonConvert.SerializeObject(machine) + "}";
                    var httpresponse = httpClient.PostAsJsonAsync("api/machine", jsonString).Result;

                    i++;

                }

                while (j < agentsDB.Count)
                {
                    //delete
                    httpClient.DeleteAsync("api/machine/" + agentsDB[j].Id);

                    j++;
                }
            }
            catch(Exception e)
            {
                System.Diagnostics.Debug.WriteLine(e.Message);
            }
        }

      
    }
}

using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.Net.Http;
using System.Text;
using TrackMate.Models;

namespace TrackMate.Services
{
    public class MachineBackgroundService : BackgroundService
    {
        private Timer _timer;
        private HttpClient httpClient { get; set; }

        private string PAT { get; set; } = "";

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            httpClient = new HttpClient();
            httpClient.BaseAddress = new Uri("http://api:443/");

            httpClient.DefaultRequestHeaders.Add("Authorization", "Basic " + Convert.ToBase64String(Encoding.ASCII.GetBytes(":" + PAT)));
            httpClient.DefaultRequestHeaders.Accept.Add(new System.Net.Http.Headers.MediaTypeWithQualityHeaderValue("application/json"));
            await Task.Delay(1000);
            _timer = new Timer(ReloadAgents, null, TimeSpan.Zero, TimeSpan.FromMinutes(5));
        }

        private void ReloadAgents(object? state)
        {
            try
            {
                List<Machine>? agentsDB = httpClient.GetFromJsonAsync<List<Machine>>("api/machine/agents").Result.OrderBy(o => o.Name).ToList();
                var resp = httpClient.GetAsync("https://dev.azure.com/ni/_apis/distributedtask/pools/426/agents?api-version=7.1-preview.1").Result;
                JObject response = JObject.Parse(resp.Content.ReadAsStringAsync().Result);
                string responseBody = response.GetValue("value")?.ToString() ?? "";
                List<Agent> agentsAPI = JsonConvert.DeserializeObject<List<Agent>>(responseBody);
                agentsAPI = agentsAPI.OrderBy(o => o.Name).ToList();

                int x = 0;
                while(x < agentsAPI.Count)
                {
                    if (!agentsAPI[x].Name.StartsWith("TS-T"))
                    {
                        agentsAPI.Remove(agentsAPI[x]);
                    }
                    else
                    {
                        x++;
                    }
                }
                int i = 0, j = 0;
                while (i < agentsAPI.Count && j < agentsDB.Count)
                {
                    string agentName = agentsDB[j].Name;
                    if (agentName != null)
                    {
                        if (agentsAPI[i].Name.CompareTo(agentName) < 0 )
                        {
                            //insert at j
                            Machine machine = new Machine();
                            machine.AgentId = agentsAPI[i].Id.ToString();
                            machine.Name = agentsAPI[i].Name;
                            machine.Status = "Available";
                            machine.Type = "Virtual";
                            machine.Comments = "None";
                            machine.LastAccessed = DateTime.UtcNow;
                            if (string.Compare(machine.Name, "TS-TEST00") == 1 && string.Compare(machine.Name, "TS-TEST22") == -1)
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
                        //If the agent goes out of pool, it will automatically deleted. Since it also erases the assign status, we are skipping the step
                        else if (agentsAPI[i].Name.CompareTo(agentName) > 0)
                        {
                            //delete at j
                            //httpClient.DeleteAsync("api/machine/" + agentsDB[j].Id);
                            j++;
                        }
                        else
                        {
                            if (agentsAPI[i].Id != int.Parse(agentsDB[j].AgentId))
                            {
                                agentsDB[j].AgentId = agentsAPI[i].Id.ToString();
                                var jsonString = "{'machine' : " + JsonConvert.SerializeObject(agentsDB[j]) + "}";
                                var httpresponse = httpClient.PutAsJsonAsync("api/machine", jsonString).Result;
                            }
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
                    //httpClient.DeleteAsync("api/machine/" + agentsDB[j].Id);

                    j++;
                }
            }
            catch (Exception e)
            {
                System.Diagnostics.Debug.WriteLine(e.Message);
            }
        }


    }
}

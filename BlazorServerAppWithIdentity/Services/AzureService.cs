using Azure.Core;
using BlazorServerAppWithIdentity.Models;
using BlazorServerAppWithIdentity.Pages;
using Microsoft.AspNetCore.Components;
using Microsoft.Extensions.Caching.Memory;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.Security.Claims;
using System.Text;

namespace BlazorServerAppWithIdentity.Services
{
    public class AzureService
    {
        private readonly HttpClient httpClient;
        private readonly IHttpContextAccessor httpContextAccessor;
        private readonly IConfiguration _configuration;
        private readonly IMemoryCache _memoryCache;
        private IGlobalStateService GlobalStateService;
        private UserService UserService;
        private NavigationManager NavigationManager;
        public AzureService(NavigationManager NavigationManager, UserService UserService, IGlobalStateService GlobalStateService, HttpClient httpClient, IConfiguration configuration, IHttpContextAccessor httpContextAccessor, IMemoryCache memoryCache)
        {
            this.httpClient = httpClient;
            _configuration = configuration;
            this.httpContextAccessor = httpContextAccessor;
            this._memoryCache = memoryCache;
            this.GlobalStateService = GlobalStateService;
            this.UserService = UserService;
            this.NavigationManager = NavigationManager;

            string svcCredentials = Convert.ToBase64String(ASCIIEncoding.ASCII.GetBytes(httpContextAccessor.HttpContext?.User?.Identity?.Name + ":" + GlobalStateService.PAT));

            httpClient.DefaultRequestHeaders.Add("Authorization", "Basic " + svcCredentials);
            httpClient.DefaultRequestHeaders.Accept.Add(
                  new System.Net.Http.Headers.MediaTypeWithQualityHeaderValue("application/json"));
        }

        public async Task<List<Pipeline>> GetPipelines()
        {
            try
            {
                string key = "Pipelines_List";
                var encodedCache = _memoryCache.Get(key);

                if (_memoryCache.TryGetValue(key, out List<Pipeline> pipelinesCache))
                {
                    return pipelinesCache;
                }
                else
                {
                    var options = new MemoryCacheEntryOptions()
                        .SetSlidingExpiration(TimeSpan.FromMinutes(30))
                        .SetAbsoluteExpiration(DateTime.Now.AddMinutes(60));

                    var resp = await httpClient.GetAsync("https://dev.azure.com/ni/DevCentral/_apis/pipelines?api-version=7.0");
                    JObject response = JObject.Parse(resp.Content.ReadAsStringAsync().Result);
                    string responseBody = response.GetValue("value").ToString();

                    List<Pipeline> pipelines = JsonConvert.DeserializeObject<List<Pipeline>>(responseBody);
                    pipelines = pipelines.Where(p =>
                p.Folder.Contains("\\eng\\IaC\\TestStand", StringComparison.OrdinalIgnoreCase)).ToList();

                    _memoryCache.Set(key, pipelines, options);
                    return pipelines;
                }

            }
            catch (Exception)
            {
                return new List<Pipeline>();
            }

        }

        public async Task<int> GetPipelinesCount()
        {
            try
            {
                List<Pipeline> pl = await GetPipelines();
                return pl.Count;
            }
            catch (Exception)
            {
                return 0;
            }
        }

        public HttpResponseMessage RunPipeline(string jsonString, string pipelineId)
        {
            try
            {
                var content = new StringContent(jsonString, Encoding.UTF8, "application/json");
                var response = httpClient.PostAsync("https://dev.azure.com/ni/DevCentral/_apis/pipelines/" + pipelineId + "/runs?api-version=7.0", content);
                return response.Result;
            }
            catch
            {
                return null;
            }

        }

        public async Task<List<Run>> GetRunsWithPipelineId(string pipelineId)
        {
            try
            {
                var resp = await httpClient.GetAsync("https://dev.azure.com/ni/DevCentral/_apis/pipelines/" + pipelineId + "/runs?api-version=7.0");
                JObject response = JObject.Parse(resp.Content.ReadAsStringAsync().Result);
                string responseBody = response.GetValue("value").ToString();

                List<Run> runsList = JsonConvert.DeserializeObject<List<Run>>(responseBody);
                runsList = runsList.GetRange(0, Math.Min(100, runsList.Count));
                return runsList;

            }
            catch (Exception)
            {
                return new List<Run>();
            }

        }

        public async Task<List<Agent>> GetAgents()
        {
            try
            {
                var resp = await httpClient.GetAsync("https://dev.azure.com/ni/_apis/distributedtask/pools/426/agents?includeAssignedRequest=true&api-version=7.1-preview.1");
                JObject response = JObject.Parse(resp.Content.ReadAsStringAsync().Result);
                string responseBody = response.GetValue("value").ToString();

                List<Agent> agents = JsonConvert.DeserializeObject<List<Agent>>(responseBody);
                agents = agents.OrderBy(o => o.Name).ToList();
                return agents;

            }
            catch (Exception)
            {
                return new List<Agent>();
            }

        }
        public List<Agent> GetAgentsWithCapability()
        {
            try
            {
                var resp = httpClient.GetAsync("https://dev.azure.com/ni/_apis/distributedtask/pools/426/agents?includeCapabilities=true&api-version=7.1-preview.1").Result;
                JObject response = JObject.Parse(resp.Content.ReadAsStringAsync().Result);
                string responseBody = response.GetValue("value").ToString();
                List<Agent> agents = JsonConvert.DeserializeObject<List<Agent>>(responseBody);
                agents = agents.OrderBy(o => o.Name).ToList();
                return agents;
            }
            catch (Exception)
            {
                return new List<Agent>();
            }

        }
        public async Task<Agent> GetAgent(string agentId)
        {
            try
            {
                var resp = await httpClient.GetAsync("https://dev.azure.com/ni/_apis/distributedtask/pools/426/agents/" + agentId + "?includeAssignedRequest=true&api-version=7.1-preview.1");
                JObject response = JObject.Parse(resp.Content.ReadAsStringAsync().Result);
                return JsonConvert.DeserializeObject<Agent>(response.ToString());
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine("exception " + ex);
            }
            return new Agent();
        }

        public async Task<AgentCapability> GetAgentCapability(string AgentId)
        {
            try
            {
                var resp = await httpClient.GetAsync("https://dev.azure.com/ni/_apis/distributedtask/pools/426/agents/" + AgentId + "?includeCapabilities=true&api-version=7.1-preview.1");
                JObject response = JObject.Parse(resp.Content.ReadAsStringAsync().Result);
                string responseBody = response.GetValue("systemCapabilities").ToString();

                AgentCapability agentCapability = JsonConvert.DeserializeObject<AgentCapability>(responseBody);
                return agentCapability;

            }
            catch (Exception)
            {
                return new AgentCapability();
            }
        }



    }
}

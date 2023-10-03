using Azure.Core;
using Blazored.LocalStorage;
using BlazorServerAppWithIdentity.Models;
using BlazorServerAppWithIdentity.Pages;
using Microsoft.AspNetCore.Components;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;
using Microsoft.JSInterop;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.Net.Http.Headers;
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
        private RunsLogService RunsLogService;
        private NavigationManager NavigationManager;
        private ILocalStorageService LocalStorage;
        private string userName;
        public AzureService(ILocalStorageService localStorage ,NavigationManager NavigationManager, RunsLogService RunsLogService, HttpClient httpClient, IConfiguration configuration, IHttpContextAccessor httpContextAccessor, IMemoryCache memoryCache)
        {
            this.httpClient = httpClient;
            _configuration = configuration;
            this.httpContextAccessor = httpContextAccessor;
            this._memoryCache = memoryCache;
            this.RunsLogService = RunsLogService;
            this.NavigationManager = NavigationManager;
            this.LocalStorage = localStorage;
            httpClient.DefaultRequestHeaders.Accept.Add(
                  new System.Net.Http.Headers.MediaTypeWithQualityHeaderValue("application/json"));
            if(httpContextAccessor.HttpContext != null && httpContextAccessor.HttpContext.User.Identity.IsAuthenticated ) 
            {
                userName = httpContextAccessor.HttpContext.User.Identity.Name??"";
                AddTokenHeader();
            }
            
        }
        public async Task AddTokenHeader()
        {
            if ((await LocalStorage.GetItemAsStringAsync("AzDoToken")) == null)
            {
                GetToken();
            }
            else
            {
                httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", (await LocalStorage.GetItemAsStringAsync("AzDoToken")));
            }
        }
        public void GetToken()
        {
            NavigationManager.NavigateTo("https://app.vssps.visualstudio.com/oauth2/authorize?client_id=E9597257-B864-4E34-B3CE-5D62DF77E7DD&response_type=Assertion&redirect_uri=https://localhost:7195/authorize&scope=vso.agentpools_manage vso.build_execute vso.pipelineresources_manage&state=state");
        }
        
        public async Task GetRefreshToken()
        {
            string refreshToken = (await LocalStorage.GetItemAsStringAsync("RefreshToken"));
            var nvc = new List<KeyValuePair<string, string>>();
            nvc.Add(new KeyValuePair<string, string>("assertion", refreshToken));
            nvc.Add(new KeyValuePair<string, string>("redirect_uri", "https://localhost:7195/authorize"));
            nvc.Add(new KeyValuePair<string, string>("client_assertion", "eyJ0eXAiOiJKV1QiLCJhbGciOiJSUzI1NiIsIng1dCI6Im9PdmN6NU1fN3AtSGpJS2xGWHo5M3VfVjBabyJ9.eyJjaWQiOiJlOTU5NzI1Ny1iODY0LTRlMzQtYjNjZS01ZDYyZGY3N2U3ZGQiLCJjc2kiOiIyNTlkZWI0OS0wN2MxLTRlYjgtYjgzZS03NTM2NmI1ZmU5MmQiLCJuYW1laWQiOiI2ZTU3MzhhMS02MmZkLTY0MzUtOWQ1ZC0xNjM4YzcxNjI4ZmUiLCJpc3MiOiJhcHAudnN0b2tlbi52aXN1YWxzdHVkaW8uY29tIiwiYXVkIjoiYXBwLnZzdG9rZW4udmlzdWFsc3R1ZGlvLmNvbSIsIm5iZiI6MTY5NTEwMDg4NiwiZXhwIjoxODUyOTUzNjg2fQ.qKQ5NQ16j8F-WglNjYVto18B3C80D-VUxN6MqknULDt5cQELAALJ6JKPbOsd3oaGnqbUjXusOiX-OiidO1tWIcBElFsufzxdcyinbUIqXVmR_rihab3gzlg3fICKP-0D-vYSIV35JglEEHUW6Bq8p3bbufecQNsvK3jOz3_8TDmjUCodZBSHvKSKtqGmvnuqFzF5Ae3yQAEGasrEWxQAh__Y4ozJ4RGcYE6UqT2y5LnStJk4XuZYvYZFt9CYvxeJbZFKSMQ5_7IDeF6mCLuwCI3GntQDccjjkDS_ecR9J3A5Z_2fCv871OjZVfdQ9ndmCzK-RoO2vHJKLvTUkaVg3w"));
            nvc.Add(new KeyValuePair<string, string>("grant_type", "refresh_token"));
            nvc.Add(new KeyValuePair<string, string>("client_assertion_type", "urn:ietf:params:oauth:client-assertion-type:jwt-bearer"));
            try
            {
                using var client = new HttpClient();
                using var req = new HttpRequestMessage(HttpMethod.Post, "https://app.vssps.visualstudio.com/oauth2/token")
                {
                    Content = new FormUrlEncodedContent(nvc)
                };

                using var resp = client.SendAsync(req).Result;
                JObject response = JObject.Parse(resp.Content.ReadAsStringAsync().Result);
                string responseBody = response.GetValue("access_token").ToString();
                await LocalStorage.SetItemAsStringAsync("AzDoToken", responseBody);
                await LocalStorage.SetItemAsStringAsync("RefreshToken", response.GetValue("refresh_token").ToString());
                httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", responseBody);
                
            }
            catch (Exception)
            {

            }

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

                    var resp = await httpClient.GetAsync("https://dev.azure.com/ni/DevCentral/_apis/pipelines?api-version=7.1-preview.1");
                    if(resp.StatusCode == System.Net.HttpStatusCode.NonAuthoritativeInformation)
                    {
                        GetRefreshToken();
                        resp = await httpClient.GetAsync("https://dev.azure.com/ni/DevCentral/_apis/pipelines?api-version=7.1-preview.1");
                    }
                    if (resp.StatusCode == System.Net.HttpStatusCode.OK)
                    {
                        JObject response = JObject.Parse(resp.Content.ReadAsStringAsync().Result);
                        string responseBody = response.GetValue("value").ToString();

                        List<Pipeline> pipelines = JsonConvert.DeserializeObject<List<Pipeline>>(responseBody);
                        pipelines = pipelines.Where(p =>
                    p.Folder.Contains("\\eng\\IaC\\TestStand", StringComparison.OrdinalIgnoreCase)).ToList();

                        _memoryCache.Set(key, pipelines, options);
                        return pipelines;
                    }
                    else
                    {
                        return new List<Pipeline>();
                    }
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
                var response = httpClient.PostAsync("https://dev.azure.com/ni/DevCentral/_apis/pipelines/" + pipelineId + "/runs?api-version=7.1", content).Result;
                if (response.StatusCode == System.Net.HttpStatusCode.NonAuthoritativeInformation)
                {
                    GetRefreshToken();
                    response = httpClient.PostAsync("https://dev.azure.com/ni/DevCentral/_apis/pipelines/" + pipelineId + "/runs?api-version=7.1", content).Result;
                }

                if (response.IsSuccessStatusCode)
                {
                    JObject resp = JObject.Parse(response.Content.ReadAsStringAsync().Result);
                    RunsLog log = new RunsLog
                    {
                        RunId = resp["id"].ToString(),
                        PipelineId = pipelineId,
                        UserName = userName
                    };
                    RunsLogService.AddRunsLog(log);
                }
                return response;
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
                var resp = await httpClient.GetAsync("https://dev.azure.com/ni/DevCentral/_apis/pipelines/" + pipelineId + "/runs?api-version=7.1");
                if (resp.StatusCode == System.Net.HttpStatusCode.NonAuthoritativeInformation)
                {
                    GetRefreshToken();
                    resp = await httpClient.GetAsync("https://dev.azure.com/ni/DevCentral/_apis/pipelines/" + pipelineId + "/runs?api-version=7.1");
                }
                if (resp.StatusCode == System.Net.HttpStatusCode.OK)
                {
                    JObject response = JObject.Parse(resp.Content.ReadAsStringAsync().Result);
                    string responseBody = response.GetValue("value").ToString();
                    List<Run> runsList = JsonConvert.DeserializeObject<List<Run>>(responseBody);
                    runsList = runsList.GetRange(0, Math.Min(100, runsList.Count));
                    return runsList;
                }
                else
                {
                    return new List<Run>();
                }

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
                if (resp.StatusCode == System.Net.HttpStatusCode.NonAuthoritativeInformation)
                {
                    GetRefreshToken();
                    resp = await httpClient.GetAsync("https://dev.azure.com/ni/_apis/distributedtask/pools/426/agents?includeAssignedRequest=true&api-version=7.1-preview.1");
                }
                if (resp.StatusCode == System.Net.HttpStatusCode.OK)
                {
                    JObject response = JObject.Parse(resp.Content.ReadAsStringAsync().Result);
                    string responseBody = response.GetValue("value").ToString();

                    List<Agent> agents = JsonConvert.DeserializeObject<List<Agent>>(responseBody);
                    agents = agents.OrderBy(o => o.Name).ToList();
                    return agents;
                }
                else
                {
                    return new List<Agent>();
                }

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
                if (resp.StatusCode == System.Net.HttpStatusCode.NonAuthoritativeInformation)
                {
                    GetRefreshToken();
                    resp = httpClient.GetAsync("https://dev.azure.com/ni/_apis/distributedtask/pools/426/agents?includeCapabilities=true&api-version=7.1-preview.1").Result;
                }
                if (resp.StatusCode == System.Net.HttpStatusCode.OK)
                {
                    JObject response = JObject.Parse(resp.Content.ReadAsStringAsync().Result);
                    string responseBody = response.GetValue("value").ToString();
                    List<Agent> agents = JsonConvert.DeserializeObject<List<Agent>>(responseBody);
                    agents = agents.OrderBy(o => o.Name).ToList();
                    return agents;
                }
                else
                {
                    return new List<Agent>();
                }
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
                if (resp.StatusCode == System.Net.HttpStatusCode.NonAuthoritativeInformation)
                {
                    GetRefreshToken();
                    resp = await httpClient.GetAsync("https://dev.azure.com/ni/_apis/distributedtask/pools/426/agents/" + agentId + "?includeAssignedRequest=true&api-version=7.1-preview.1");
                }
                if (resp.StatusCode == System.Net.HttpStatusCode.OK)
                {
                    JObject response = JObject.Parse(resp.Content.ReadAsStringAsync().Result);
                    return JsonConvert.DeserializeObject<Agent>(response.ToString());
                }
                else
                {
                    return new Agent();
                }
            }
            catch (Exception)
            {
                return new Agent();
            }
        }

        public async Task<AgentCapability> GetAgentCapability(string AgentId)
        {
            try
            {
                var resp = await httpClient.GetAsync("https://dev.azure.com/ni/_apis/distributedtask/pools/426/agents/" + AgentId + "?includeCapabilities=true&api-version=7.1-preview.1");

                if (resp.StatusCode == System.Net.HttpStatusCode.NonAuthoritativeInformation)
                {
                    GetRefreshToken();
                    resp = await httpClient.GetAsync("https://dev.azure.com/ni/_apis/distributedtask/pools/426/agents/" + AgentId + "?includeCapabilities=true&api-version=7.1-preview.1");
                }
                if (resp.StatusCode == System.Net.HttpStatusCode.OK)
                {
                    JObject response = JObject.Parse(resp.Content.ReadAsStringAsync().Result);
                    string responseBody = response.GetValue("systemCapabilities").ToString();

                    AgentCapability agentCapability = JsonConvert.DeserializeObject<AgentCapability>(responseBody);
                    return agentCapability;
                }
                else
                {
                    return new AgentCapability();
                }

            }
            catch (Exception)
            {
                return new AgentCapability();
            }
        }



    }
}

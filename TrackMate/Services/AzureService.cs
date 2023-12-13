using Azure.Core;
using TrackMate.Pages;
using Microsoft.AspNetCore.Components;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;
using Microsoft.JSInterop;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.Net.Http.Headers;
using System.Security.Claims;
using System.Text;
using TrackMate.Models;
using NuGet.ProjectModel;
using System.IO;

namespace TrackMate.Services
{
    public class AzureService
    {
        private readonly HttpClient httpClient;
        private readonly IHttpContextAccessor httpContextAccessor;
        private readonly IConfiguration _configuration;
        private readonly IMemoryCache _memoryCache;
        private RunsLogService RunsLogService;
        private NavigationManager NavigationManager;
        private IJSRuntime JSRuntime;
        private string userName;
        public AzureService(NavigationManager NavigationManager, RunsLogService RunsLogService, HttpClient httpClient, IConfiguration configuration, IHttpContextAccessor httpContextAccessor, IMemoryCache memoryCache, IJSRuntime jSRuntime)
        {
            this.httpClient = httpClient;
            _configuration = configuration;
            this.httpContextAccessor = httpContextAccessor;
            _memoryCache = memoryCache;
            this.RunsLogService = RunsLogService;
            this.NavigationManager = NavigationManager;
            httpClient.DefaultRequestHeaders.Accept.Add(
                  new MediaTypeWithQualityHeaderValue("application/json"));
            if (httpContextAccessor.HttpContext != null && httpContextAccessor.HttpContext.User.Identity.IsAuthenticated)
            {
                userName = httpContextAccessor.HttpContext.User.Identity.Name ?? "";
                AddTokenHeader();
            }
            JSRuntime = jSRuntime;
        }
        public async Task AddTokenHeader()
        {
            if (await JSRuntime.InvokeAsync<string>("localStorage.getItem", "AzDoToken") == null)
            {
                GetToken();
            }
            else
            {
                httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", await JSRuntime.InvokeAsync<string>("localStorage.getItem", "AzDoToken"));
            }
        }
        public void GetToken()
        {
            NavigationManager.NavigateTo("https://app.vssps.visualstudio.com/oauth2/authorize?client_id=A4A3E059-1083-4CFE-BCDC-18696F9A716C&response_type=Assertion&redirect_uri=https://inrd-tstest01:7195/authorize&scope=vso.agentpools_manage vso.build_execute vso.code_full vso.dashboards_manage vso.graph_manage vso.notification_manage vso.pipelineresources_manage vso.profile_write vso.project_manage vso.taskgroups_write vso.tokenadministration vso.work_full&state=state");
            //NavigationManager.NavigateTo("https://app.vssps.visualstudio.com/oauth2/authorize?client_id=A4A3E059-1083-4CFE-BCDC-18696F9A716C&response_type=Assertion&redirect_uri=https://localhost:7195/authorize&scope=vso.agentpools_manage vso.build_execute vso.code_full vso.dashboards_manage vso.graph_manage vso.notification_manage vso.pipelineresources_manage vso.profile_write vso.project_manage vso.taskgroups_write vso.tokenadministration vso.work_full&state=state");
        }

        public async Task GetRefreshToken()
        {
            string refreshToken = await JSRuntime.InvokeAsync<string>("localStorage.getItem", "RefreshToken");
            var nvc = new List<KeyValuePair<string, string>>();
            nvc.Add(new KeyValuePair<string, string>("assertion", refreshToken));
            nvc.Add(new KeyValuePair<string, string>("redirect_uri", "https://inrd-tstest01:7195/authorize"));
            //nvc.Add(new KeyValuePair<string, string>("redirect_uri", "https://localhost:7195/authorize"));
            nvc.Add(new KeyValuePair<string, string>("client_assertion", "eyJ0eXAiOiJKV1QiLCJhbGciOiJSUzI1NiIsIng1dCI6Im9PdmN6NU1fN3AtSGpJS2xGWHo5M3VfVjBabyJ9.eyJjaWQiOiJhNGEzZTA1OS0xMDgzLTRjZmUtYmNkYy0xODY5NmY5YTcxNmMiLCJjc2kiOiIwNzM5ZmE3OC1lMzk0LTQwYTEtODQ1Ny1hZWU2YmU1MDVjOWUiLCJuYW1laWQiOiI2ZTU3MzhhMS02MmZkLTY0MzUtOWQ1ZC0xNjM4YzcxNjI4ZmUiLCJpc3MiOiJhcHAudnN0b2tlbi52aXN1YWxzdHVkaW8uY29tIiwiYXVkIjoiYXBwLnZzdG9rZW4udmlzdWFsc3R1ZGlvLmNvbSIsIm5iZiI6MTY5ODIwODgyMSwiZXhwIjoxODU2MDYxNjIxfQ.I_14AciVtAf_DLKIjWqofLWzqsTIattsQViIWj4vB0lnrZp_noAEVxK7KlA83xzOaHaw9ABFdBUYc4ekr-2VRZeISk37XF4AFql4JQ9VSdntxkbmuBlMpi5MV1bt-gqcI918Uth5JSjUKGjWxl9dqk2QPij9l2YXsU9AauWfarC-Bf-lmtg7xjjXqllUMgvfu1LrUTQZ4WkR8ya8TNTi8xrA6e2crnMx3nE3GDz_wfv3MhSkR7MYuxZmES_z42Rre2PJI7ERUtO4GykU9cFlznfzas-V4ccW-NTFQpG9GStM1KcPWT6hnCmUdl2mfmH7VMnu-yxFha-OI6mRPNjnxQ"));
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
                //await LocalStorage.SetItemAsStringAsync("AzDoToken", responseBody);
                //await LocalStorage.SetItemAsStringAsync("RefreshToken", response.GetValue("refresh_token").ToString());
                await JSRuntime.InvokeVoidAsync("localStorage.setItem", "AzDoToken", responseBody);
                await JSRuntime.InvokeVoidAsync("localStorage.setItem", "RefreshToken", response.GetValue("refresh_token").ToString());
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
                    if (resp.StatusCode == System.Net.HttpStatusCode.NonAuthoritativeInformation)
                    {
                        await GetRefreshToken();
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
        public async Task<List<String>> GetPipelinePath(string pipelineId)
        {
            try
            {
                    var resp = await httpClient.GetAsync("https://dev.azure.com/ni/DevCentral/_apis/pipelines/"+pipelineId+"?api-version=7.1-preview.1");
                    if (resp.StatusCode == System.Net.HttpStatusCode.NonAuthoritativeInformation)
                    {
                        await GetRefreshToken();
                        resp = await httpClient.GetAsync("https://dev.azure.com/ni/DevCentral/_apis/pipelines/"+pipelineId+"?api-version=7.1-preview.1");
                    }
                    if (resp.StatusCode == System.Net.HttpStatusCode.OK)
                    {
                        JObject response = JObject.Parse(resp.Content.ReadAsStringAsync().Result);
                        string repositoryId = response.GetValue("configuration").ToObject<JObject>().GetValue("repository").ToObject<JObject>().GetValue("id").ToString();
                        string path = response.GetValue("configuration").ToObject<JObject>().GetValue("path").ToString();
                        string repositoryName = await GetRepositoryNameById(repositoryId);

                        return new List<string> { repositoryName, path };
                    }
                    else
                    {
                        return new List<string>();
                    }
                

            }
            catch (Exception)
            {
                return new List<string>();
            }

        }
        public async Task<string> GetRepositoryNameById(string repoId)
        {
            try
            {

                var resp = await httpClient.GetAsync("https://dev.azure.com/ni/devcentral/_apis/git/repositories/" + repoId + "?api-version=7.1");
                if (resp.StatusCode == System.Net.HttpStatusCode.NonAuthoritativeInformation)
                {
                    await GetRefreshToken();
                    resp = await httpClient.GetAsync("https://dev.azure.com/ni/devcentral/_apis/git/repositories/" + repoId + "?api-version=7.1");
                }
                if (resp.StatusCode == System.Net.HttpStatusCode.OK)
                {
                    JObject response = JObject.Parse(resp.Content.ReadAsStringAsync().Result);
                    string repoName = response.GetValue("name").ToString();
                    return repoName;
                }
                else
                {
                    return string.Empty;
                }


            }
            catch (Exception)
            {
                return string.Empty;
            }
        }
        public async Task<String> GetYamlFile(List<string> location)
        {
            try
            {
                if (location[0] == string.Empty || location[1] == string.Empty) 
                {
                    return string.Empty;
                }

                var resp = await httpClient.GetAsync("https://dev.azure.com/ni/DevCentral/_apis/git/repositories/" + location[0] + "/items?path=" + location[1] + "&$format=json&includeContent=true&api-version=7.1-preview.1");
                if (resp.StatusCode == System.Net.HttpStatusCode.NonAuthoritativeInformation)
                {
                    await GetRefreshToken();
                    resp = await httpClient.GetAsync("https://dev.azure.com/ni/DevCentral/_apis/git/repositories/" + location[0] + "/items?path=" + location[1] + "&$format=json&includeContent=true&api-version=7.1-preview.1");
                }
                if (resp.StatusCode == System.Net.HttpStatusCode.OK)
                {
                    JObject response = JObject.Parse(resp.Content.ReadAsStringAsync().Result);
                    string yamlContent = response.GetValue("content").ToString();
                    return yamlContent;
                }
                else
                {
                    return string.Empty;
                }


            }
            catch (Exception)
            {
                return string.Empty;
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
                    await GetRefreshToken();
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
                    await GetRefreshToken();
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
        public async Task<List<Agent>> GetAgentsWithCapability()
        {
            try
            {
                var resp = httpClient.GetAsync("https://dev.azure.com/ni/_apis/distributedtask/pools/426/agents?includeCapabilities=true&api-version=7.1-preview.1").Result;
                if (resp.StatusCode == System.Net.HttpStatusCode.NonAuthoritativeInformation)
                {
                    await GetRefreshToken();
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
                    await GetRefreshToken();
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
                    await GetRefreshToken();
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
                    return null;
                }

            }
            catch (Exception)
            {
                return null;
            }
        }



    }
}

using Microsoft.AspNetCore.SignalR;
using BlazorServerAppWithIdentity.Hubs;
using Machine = BlazorServerAppWithIdentity.Models.Machine;
using Newtonsoft.Json;
using System.Net.Http.Headers;
using Newtonsoft.Json.Linq;
using Azure;
using System.Collections.Generic;
using Blazored.LocalStorage;

namespace BlazorServerAppWithIdentity.Services
{
    
    public class MachineService
    {
        private readonly HttpClient httpClient;
        private readonly IHubContext<BlazorHub> _hubContext;
        protected ILocalStorageService LocalStorageService;
        public MachineService(ILocalStorageService LocalStorageService, HttpClient httpClient, IHubContext<BlazorHub> hubContext)
        {
            this.httpClient = httpClient;
            _hubContext = hubContext;
            this.LocalStorageService = LocalStorageService;
        }

        public IEnumerable<Models.Machine> GetMachines()
        {
            IEnumerable <Machine> emptyObject = new List<Models.Machine>();
            return httpClient.GetFromJsonAsync<List<Machine>>("api/machine").Result ?? emptyObject;
        }
        public IEnumerable<Models.Machine> GetVirtualMachines()
        {
            IEnumerable<Machine> emptyObject = new List<Models.Machine>();
            return httpClient.GetFromJsonAsync<List<Machine>>("api/machine/agents").Result ?? emptyObject;
        }
        public Machine GetVirtualMachineByName(string machineName)
        {
            return httpClient.GetFromJsonAsync<List<Machine>>("api/machine/agents").Result?.Find(o => o.Name == machineName) ?? new Machine();
        }
        //returns a list of agents that are reserved by current user or available
        public IEnumerable<Models.Machine> GetReservedMachines(string userName)
        {
            return httpClient.GetFromJsonAsync<List<Machine>>("api/machine/agents/reserved/" + userName).Result;
        }
        public int GetMachineCount()
        {
            return httpClient.GetFromJsonAsync<int>("api/machine/count").Result;
        }

        public int GetAvailableMachineCount()
        {
            return httpClient.GetFromJsonAsync<int>("api/machine/available/count").Result;
        }

        public Machine GetMachineById(string id)
        {
            return httpClient.GetFromJsonAsync<Machine>("api/machine/" + id).Result?? new Machine();
            
        }
        public Machine GetMachineByAgentId(string agentId)
        {
            return httpClient.GetFromJsonAsync<Machine>("api/machine/agent/" + agentId).Result ?? new Machine();
        }
        public string AddMachine(Models.Machine machine)
        {
            var jsonString = "{'machine' :" + JsonConvert.SerializeObject(machine) + "}";
            var response = httpClient.PostAsJsonAsync("api/machine", jsonString).Result;
            JObject responseJson = JObject.Parse(response.Content.ReadAsStringAsync().Result);
            return responseJson["Result"]?.ToString() ?? "Failed Operation!";
        }

        public String UpdateMachine(Models.Machine machine)
        {
            var jsonString = "{'machine' :" + JsonConvert.SerializeObject(machine) + "}";

            var response = httpClient.PutAsJsonAsync("api/machine", jsonString).Result;
            JObject responseJson = JObject.Parse(response.Content.ReadAsStringAsync().Result);
            return responseJson["Result"]?.ToString() ?? "Failed Operation";
        }

        public String RemoveMachine(String id)
        {
            var response = httpClient.DeleteAsync("api/machine/"+id).Result;
            JObject responseJson = JObject.Parse(response.Content.ReadAsStringAsync().Result);
            return responseJson["Result"]?.ToString() ?? "Failed Operation";
        }

        public String AssignUser(string userId, string userName, string comments, DateTime endTime, Machine machine)
        {
            var jsonString = "{'machine' :" + JsonConvert.SerializeObject(machine) + ", 'userId' : '" + userId + "', 'userName' : '" + userName + "', 'comments' : '"+comments+"', 'endTime' : '" + endTime + "'}";
            var response = httpClient.PostAsJsonAsync("api/machine/assign", jsonString).Result;
            JObject responseJson = JObject.Parse(response.Content.ReadAsStringAsync().Result);
            return responseJson["Result"]?.ToString() ?? "Failed Operation";
        }

        public string RevokeUser(Machine machine)
        {
            var jsonString = "{'machine' :" + JsonConvert.SerializeObject(machine) + "}";           
            var response = httpClient.PutAsJsonAsync("api/machine/revoke", jsonString).Result;

            JObject responseJson = JObject.Parse(response.Content.ReadAsStringAsync().Result);
            return responseJson["Result"]?.ToString() ?? "Failed Operation";
        }

        public List<Machine> GetMachinesByUserId(string id)
        {
            return httpClient.GetFromJsonAsync<List<Machine>>("api/machine/user/" + id).Result ?? new List<Machine>();
        }



    }
}

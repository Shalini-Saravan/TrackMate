using Microsoft.AspNetCore.SignalR;
using Machine = TrackMate.Models.Machine;
using Newtonsoft.Json;
using System.Net.Http.Headers;
using Newtonsoft.Json.Linq;
using Azure;
using System.Collections.Generic;
using Blazored.LocalStorage;
using TrackMate.Hubs;

namespace TrackMate.Services
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

        public IEnumerable<Machine> GetMachines()
        {
            IEnumerable<Machine> emptyObject = new List<Machine>();
            return httpClient.GetFromJsonAsync<List<Machine>>("api/machine").Result ?? emptyObject;
        }
        public IEnumerable<Machine> GetVirtualMachines()
        {
            IEnumerable<Machine> emptyObject = new List<Machine>();
            return httpClient.GetFromJsonAsync<List<Machine>>("api/machine/agents").Result ?? emptyObject;
        }
        public Machine GetVirtualMachineByName(string machineName)
        {
            machineName = machineName.ToLower();
            return httpClient.GetFromJsonAsync<List<Machine>>("api/machine/agents").Result?.Find(o => o.Name.ToLower() == machineName) ?? new Machine();
        }
        //returns a list of agents that are reserved by current user or available
        public IEnumerable<Machine> GetReservedMachines(string userName)
        {
            if (userName != null)
            {
                return httpClient.GetFromJsonAsync<List<Machine>>("api/machine/agents/reserved/" + userName).Result;
            }
            else { return new List<Machine>(); }
        }
        public IEnumerable<string> GetReservedMachinesByUser(string userName)
        {

            if (userName != null)
            {
                List<string> machines = new List<string>();
                foreach(var m in GetReservedMachines(userName))
                {
                    if(m.Status != "Available")
                    {
                        machines.Add(m.Name.ToLower());
                    }
                }
                return machines;
            }
            else { return new List<string>(); }
        }
        public IEnumerable<string> GetUnavailableMachines(string userName)
        {
            if (userName != null)
            {
                return httpClient.GetFromJsonAsync<List<string>>("api/machine/agents/unavailable/" + userName).Result;
            }
            else { return new List<string>(); }
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
            return httpClient.GetFromJsonAsync<Machine>("api/machine/" + id).Result ?? new Machine();

        }
        public Machine GetMachineByAgentId(string agentId)
        {
            return httpClient.GetFromJsonAsync<Machine>("api/machine/agent/" + agentId).Result ?? new Machine();
        }
        public string AddMachine(Machine machine)
        {
            var jsonString = "{'machine' :" + JsonConvert.SerializeObject(machine) + "}";
            var response = httpClient.PostAsJsonAsync("api/machine", jsonString).Result;
            JObject responseJson = JObject.Parse(response.Content.ReadAsStringAsync().Result);
            return responseJson["Result"]?.ToString() ?? "Failed Operation!";
        }

        public string UpdateMachine(Machine machine)
        {
            var jsonString = "{'machine' :" + JsonConvert.SerializeObject(machine) + "}";

            var response = httpClient.PutAsJsonAsync("api/machine", jsonString).Result;
            JObject responseJson = JObject.Parse(response.Content.ReadAsStringAsync().Result);
            return responseJson["Result"]?.ToString() ?? "Failed Operation";
        }

        public string UpdateAssignedMachine(Machine machine)
        {
            var jsonString = "{'machine' :" + JsonConvert.SerializeObject(machine) + "}";

            var response = httpClient.PutAsJsonAsync("api/machine/assigned", jsonString).Result;
            JObject responseJson = JObject.Parse(response.Content.ReadAsStringAsync().Result);
            return responseJson["Result"]?.ToString() ?? "Failed Operation";
        }

        public string RemoveMachine(string id)
        {
            var response = httpClient.DeleteAsync("api/machine/" + id).Result;
            JObject responseJson = JObject.Parse(response.Content.ReadAsStringAsync().Result);
            return responseJson["Result"]?.ToString() ?? "Failed Operation";
        }

        public string AssignUser(string userId, string userName, string comments, DateTime endTime, Machine machine)
        {
            var jsonString = "{'machine' :" + JsonConvert.SerializeObject(machine) + ", 'userId' : '" + userId + "', 'userName' : '" + userName + "', 'comments' : '" + comments + "', 'endTime' : '" + endTime + "'}";
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

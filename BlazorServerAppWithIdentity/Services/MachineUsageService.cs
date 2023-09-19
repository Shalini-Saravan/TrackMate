using BlazorServerAppWithIdentity.Models;
using System.Net.Http.Headers;

namespace BlazorServerAppWithIdentity.Services
{
    public class MachineUsageService
    {
        private readonly HttpClient httpClient;
        private readonly IGlobalStateService GlobalState;

        public MachineUsageService(HttpClient httpClient, IGlobalStateService globalState)
        {
            this.httpClient = httpClient;
            GlobalState = globalState;
            this.httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", GlobalState.TokenValue);

        }
        public IEnumerable<MachineUsage> GetMachineUsageByUserName(string userName)
        { 
            IEnumerable<MachineUsage> machineUsages = new List<MachineUsage>();
            return httpClient?.GetFromJsonAsync<IEnumerable<MachineUsage>>("api/machineusage/" + userName).Result ?? machineUsages;
        }

    }
}

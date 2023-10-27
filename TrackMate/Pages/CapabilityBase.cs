using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.SignalR.Client;
using System.Security.Claims;
using TrackMate.Models;
using TrackMate.Services;

namespace TrackMate.Pages
{
    public class CapabilityBase : ComponentBase
    {

        [Inject]
        MachineService? MachineService { get; set; }

        [Inject]
        AzureService? AzureService { get; set; }
        [Inject]
        UserService? UserService { get; set; }
        [Parameter]
        public string? AgentId { get; set; }
        [Parameter]
        public string? AgentName { get; set; }
        [Inject]
        public IHttpContextAccessor? HttpContextAccessor { get; set; }

        public string? message { get; set; } = string.Empty;
        public AgentCapability? agentCapability { get; set; }
        public Agent? agent { get; set; }
        public Machine? machine { get; set; }
        protected override async Task OnInitializedAsync()
        {
            base.OnInitialized();

            try
            {
                if (AzureService != null)
                {
                    agentCapability = await AzureService.GetAgentCapability(AgentId ?? "");
                    agent = await AzureService.GetAgent(AgentId ?? "");
                }
                machine = MachineService?.GetMachineByAgentId(AgentId ?? "");
            }
            catch (Exception)
            {
                agentCapability = null;
                agent = null;
            }

            if (agentCapability == null || agent == null || machine == null)
            {
                message = "Unable to fetch Agent Details!";
            }

        }
    }
}

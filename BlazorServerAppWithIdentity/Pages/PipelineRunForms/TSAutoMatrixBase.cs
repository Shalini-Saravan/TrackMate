using Azure;
using BlazorServerAppWithIdentity.Models;
using BlazorServerAppWithIdentity.Services;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.SignalR.Client;
using Newtonsoft.Json.Linq;
using System.Security.Claims;

namespace BlazorServerAppWithIdentity.Pages.PipelineRunForms
{
    public class TSAutoMatrixBase : ComponentBase
    {
        [Inject]
        public AzureService? AzureService { get; set; }
        [Inject]
        public NavigationManager? NavigationManager { get; set; }
        [Inject]
        public MachineService? MachineService { get; set; }
        [Inject]
        public IConfiguration? configuration { get; set; }
        [Inject]
        public IHttpContextAccessor? HttpContextAccessor { get; set; }

        [Parameter]
        public string? PipeLineId { get; set; }
        [Parameter]
        public string? PipeLineName { get; set; }
        public IEnumerable<Machine>? MachinesList { get; set; }

        public string Instructions { get; set; } = "N/A";
        public string? Machine_Name { get; set; }
        public string RunSetupOnly { get; set; } = "false";
        public string Bitness { get; set; } = "None";
        public string OperatorInterface { get; set; } = "None";
        public string TestType { get; set; } = "None";
        public string TierToTest { get; set; } = "None";
        public string TSVersion { get; set; } = "NA";

        public string LVVersion { get; set; } = "NA";

        public Boolean isSubmitting { get; set; } = false;
        public String notification { get; set; } = "";
        public string? runsLink { get; set; }

        protected override async Task OnInitializedAsync()
        {
            base.OnInitialized();
            runsLink = "/pipeline/" + PipeLineId + "/" + PipeLineName + "/runs";
            try
            {
                MachinesList = MachineService?.GetReservedMachines(HttpContextAccessor?.HttpContext?.User?.Identity?.Name ?? "").ToList();
            }
            catch (Exception)
            {
                MachinesList = new List<Machine>();
            }
            if (configuration != null)
            {
                var hubConnection = new HubConnectionBuilder()
                    .WithUrl(configuration["HubUrl"])
                    .Build();

                hubConnection.On<string>("MachineLoaded", OnMachineLoaded);
                await hubConnection.StartAsync();
            }
        }
        private void OnMachineLoaded(string machine)
        {
            try
            {
                MachinesList = MachineService?.GetReservedMachines(HttpContextAccessor?.HttpContext?.User?.Identity?.Name ?? "").ToList();
            }
            catch (Exception)
            {
                MachinesList = new List<Machine>();
            }
            StateHasChanged();
        }
        public void RunPipeline()
        {
            if (isSubmitting) return;
            isSubmitting = true;
            try
            {
                Machine? machine = MachineService?.GetVirtualMachineByName(Machine_Name ?? "");
                Machine_Name = Machine_Name?.ToLower();
                string jsonString = "{\"definition\":{\"id\":" + PipeLineId + "}, \"templateParameters\": {\"Instructions\" : \"" + Instructions + "\",\"Machine_Name\": \"" + Machine_Name + "\", \"Bitness\" : \"" + Bitness + "\" ,\"RunSetupOnly\" :" + RunSetupOnly + ", \"OperatorInterface\" : \"" + OperatorInterface +
                    "\" , \"Testype\" : \"" + TestType + "\",\"TierToTest\" : \"" + TierToTest + "\" ,\"TSVersion\" : \"" + TSVersion + "\",\"LVVersion\": \"" + LVVersion + "\"}}";

                var result = AzureService?.RunPipeline(jsonString, PipeLineId ?? "");
                if (result == null || !result.IsSuccessStatusCode)
                {
                    notification = "Error: Unable to run pipeline!";
                }
                else
                {
                    notification = "Pipeline Run Success!";
                    if (machine?.Status == "Available")
                    {
                        string userId = HttpContextAccessor?.HttpContext?.User?.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "";
                        MachineService?.AssignUser(userId, HttpContextAccessor?.HttpContext?.User?.Identity?.Name ?? "", "Auto Assigned", DateTime.UtcNow.AddDays(2), machine);
                    }
                }
            }
            finally { isSubmitting = false; }
        }
        protected void clearNotification()
        {
            this.notification = string.Empty;
        }
    }

}

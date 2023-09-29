using Blazored.LocalStorage;
using BlazorServerAppWithIdentity.Models;
using BlazorServerAppWithIdentity.Services;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.SignalR.Client;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.ComponentModel.DataAnnotations;
using System.Security.Claims;

namespace BlazorServerAppWithIdentity.Pages.PipelineRunForms
{
    public class DeployVMsBase : ComponentBase, IAsyncDisposable
    {
        [Inject]
        public AzureService? AzureService { get; set; }
        [Inject]
        public NavigationManager? NavigationManager { get; set; }
        [Inject]
        public MachineService? MachineService { get; set; }
        [Inject]
        public IConfiguration? Configuration { get; set; }

        [Parameter]
        public string? PipeLineId { get; set; }
        [Parameter]
        public string? PipeLineName { get; set; }
        [Inject]
        public IHttpContextAccessor HttpContextAccessor { get; set; }
        [Inject]
        public IConfiguration configuration { get; set; }
        [Inject]
        ILocalStorageService LocalStorage { get; set; }
        public IEnumerable<Machine>? MachinesList { get; set; }

        public Machine? machine { get; set; }


        public string MachineState { get; set; } = "All TSAuto mainline software";
        public string WindowsImage { get; set; } = "windows-10-baseimage";
        public string RebuildMachine { get; set; } = "Single Machine";
        public string? SingleMachineName { get; set; }

        public string Feeds { get; set; } = "[]";
        public string CustomSoftwares { get; set; } = "false";
        public string MultipleMachineNames { get; set; } = "None";
        public string Rebuild { get; set; } = "False";

        public Boolean isSubmitting { get; set; } = false;
        public string notification { get; set; } = "";
        public string? runsLink { get; set; }
        private HubConnection hubConnection { get; set; }
        protected override async Task OnInitializedAsync()
        {
            base.OnInitialized();
            string TokenValue = await LocalStorage.GetItemAsStringAsync("TokenValue");
            runsLink = "/pipeline/" + PipeLineId + "/" + PipeLineName + "/runs";
            try
            {
                MachinesList = MachineService.GetReservedMachines(HttpContextAccessor.HttpContext.User.Identity.Name).ToList();
            }
            catch (Exception)
            {
                MachinesList = new List<Machine>();
            }

            if (configuration != null)
            {
                hubConnection = new HubConnectionBuilder()
                   .WithUrl(configuration["HubUrl"], options =>
                   {
                       options.AccessTokenProvider = () => Task.FromResult(TokenValue ?? null);
                   })
                   .Build();

                hubConnection.On<string>("MachineLoaded", OnMachineLoaded);

                await hubConnection.StartAsync();
            }
        }

        private void OnMachineLoaded(string machine)
        {
            MachineService?.GetReservedMachines(HttpContextAccessor?.HttpContext?.User?.Identity?.Name ?? "").ToList();
            StateHasChanged();
        }

        public void RunPipeline()
        {
            if (isSubmitting) return;
            isSubmitting = true;
            notification = "";
            try
            {
                if (SingleMachineName == null && MultipleMachineNames == "None")
                {
                    notification = "Please choose the machine to run the pipeline!";
                }
                else
                {
                    if (SingleMachineName != null)
                    {
                        machine = MachineService?.GetVirtualMachineByName(SingleMachineName);
                        SingleMachineName = SingleMachineName.ToLower();
                    }
                    string json = JsonConvert.SerializeObject(Feeds, Formatting.Indented);
                    if (MultipleMachineNames != "None")
                    {
                        MultipleMachineNames = MultipleMachineNames.Replace("\"", "\\\"");
                    }
                    string jsonString = "{\"definition\":{\"id\":" + PipeLineId + "}, \"templateParameters\": {\"MachineState\": \"" + MachineState + "\",\"CustomSoftwares\" :" + CustomSoftwares + ", \"Feeds\" : " + json + ",\"WindowsImage\" : \"" + WindowsImage + "\",\"RebuildMachines\": \"" + RebuildMachine + "\", \"SingleMachineName\": \"" + SingleMachineName + "\", \"MultipleMachineNames\": \"" + MultipleMachineNames + "\", \"Rebuild\":\"" + Rebuild + "\"}}";

                    var result = AzureService?.RunPipeline(jsonString, PipeLineId ?? "");
                    if (result == null || !result.IsSuccessStatusCode)
                    {
                        notification = "Error: Unable to run pipeline!";
                    }
                    else
                    {
                        notification = "Pipeline Run Success!";
                        if (SingleMachineName != null)
                        {
                            if (machine?.Status == "Available")
                            {
                                string userId = HttpContextAccessor?.HttpContext?.User?.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "";
                                MachineService?.AssignUser(userId, HttpContextAccessor?.HttpContext?.User?.Identity?.Name ?? "", "Auto Assigned", DateTime.UtcNow.AddDays(2), machine);
                            }
                        }
                    }

                }
            }
            finally { isSubmitting = false; }
        }
        protected void clearNotification()
        {
            this.notification = string.Empty;
        }
        public async ValueTask DisposeAsync()
        {
            await hubConnection.DisposeAsync();
        }
    }
}

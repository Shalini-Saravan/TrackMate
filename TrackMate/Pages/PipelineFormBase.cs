using TrackMate.Services;
using Microsoft.AspNetCore.Components;
using YamlDotNet.Serialization;
using Blazored.LocalStorage;
using TrackMate.Models;
using System.Security.Claims;
using Newtonsoft.Json;
using Machine = TrackMate.Models.Machine;
using Microsoft.AspNetCore.SignalR.Client;
using System.Configuration;

namespace TrackMate.Pages
{

    public class PipelineFormBase : ComponentBase
    {

        [Parameter]
        public string? PipeLineId { get; set; }
        [Parameter]
        public string? PipeLineName { get; set; }
        [Inject]
        public AzureService AzureService { get; set; }
        [Inject]
        public ILocalStorageService LocalStorage { get; set; }
        [Inject]
        public MachineService? MachineService { get; set; }
        [Inject]
        public IHttpContextAccessor? HttpContextAccessor { get; set; }
        [Inject]
        public NavigationManager? NavigationManager { get; set; }
        [Inject]
        public IConfiguration? configuration { get; set; }
        protected List<Parameter> parameters = new List<Parameter>();

        protected string yamlData;
        protected Dictionary<string, string> formData = new Dictionary<string, string>();
        public List<string>? UnAvailableMachines { get; set; }
        public List<string>? ReservedMachines { get; set; }
        public string? runsLink { get; set; }
        public string notification { get; set; } = "";
        public string message { get; set; } = "";
        public Boolean isSubmitting { get; set; } = false;
        private HubConnection hubConnection { get; set; }
        protected override async Task OnInitializedAsync()
        {
            base.OnInitialized();
            string TokenValue = await LocalStorage.GetItemAsStringAsync("TokenValue");
            runsLink = "/pipeline/" + PipeLineId + "/" + PipeLineName + "/runs";

            List<string> location = await AzureService.GetPipelinePath(PipeLineId);
            yamlData = await AzureService.GetYamlFile(location);
            ParseYaml(yamlData);

            try
            {
                UnAvailableMachines = MachineService?.GetUnavailableMachines(HttpContextAccessor?.HttpContext?.User?.Identity?.Name ?? "").ToList();
                UnAvailableMachines = UnAvailableMachines.ConvertAll(d => d.ToLower());
                ReservedMachines = MachineService?.GetReservedMachinesByUser(HttpContextAccessor?.HttpContext?.User?.Identity?.Name ?? "").ToList();
            }
            catch (Exception)
            {
                UnAvailableMachines = new List<string>();
                ReservedMachines = new List<string>();

            }
            if (configuration != null)
            {
                var baseUri = NavigationManager?.BaseUri;
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
            try
            {
                UnAvailableMachines = MachineService?.GetUnavailableMachines(HttpContextAccessor?.HttpContext?.User?.Identity?.Name ?? "").ToList();
                UnAvailableMachines = UnAvailableMachines.ConvertAll(d => d.ToLower());
                ReservedMachines = MachineService?.GetReservedMachinesByUser(HttpContextAccessor?.HttpContext?.User?.Identity?.Name ?? "").ToList();
            }
            catch (Exception)
            {
                UnAvailableMachines = new List<string>();
                ReservedMachines = new List<string>();

            }
            StateHasChanged();
        }
        private void ParseYaml(string yamlData)
        {
            if (yamlData == string.Empty)
            {
                message = "Error: Unable to parse YAML file!";
                return;
            }

            var deserializer = new DeserializerBuilder().Build();
            using (TextReader reader = new StringReader(yamlData))
            {
                var yamlObject = deserializer.Deserialize(reader);
                if (yamlObject is Dictionary<object, object> rootDict)
                {
                    if (rootDict.ContainsKey("parameters") && rootDict["parameters"] is List<object> parametersList)
                    {
                        foreach (var param in parametersList)
                        {
                            if (param is Dictionary<object, object> paramDict)
                            {
                                var parameter = new Parameter
                                {
                                    name = paramDict["name"].ToString() ?? "",
                                    displayName = paramDict["displayName"].ToString() ?? "",

                                };
                                if (paramDict.ContainsKey("type"))
                                {
                                    parameter.type = paramDict["type"].ToString() ?? "";
                                }
                                if (paramDict.ContainsKey("default"))
                                {
                                    if (!paramDict["default"].ToString().Contains("System.Object"))
                                    {
                                        parameter.Default = paramDict["default"].ToString() ?? "";
                                    }
                                    else
                                    {
                                        parameter.Default = "[]";
                                    }

                                }

                                if (paramDict.ContainsKey("values") && paramDict["values"] is List<object> valuesList)
                                {
                                    parameter.values = valuesList.Select(v => v.ToString()).ToList();
                                }

                                parameters.Add(parameter);

                            }
                        }

                    }
                }

            }
        }
        protected void RunPipeline()
        {
            if (isSubmitting) return;
            isSubmitting = true;

            try
            {
                List<Machine> machines = new List<Machine>();
                string jsonString = "{\"definition\":{\"id\":" + PipeLineId + "}, \"templateParameters\": {";
                for (int i = 0; i < formData.Count; i++)
                {

                    if (parameters[i].type == "boolean" && parameters[i].values == null)
                    {
                        jsonString += "\"" + formData.ElementAt(i).Key + "\":" + formData.ElementAt(i).Value + ",";
                    }
                    else if (parameters[i].type == "object")
                    {
                        jsonString += "\"" + formData.ElementAt(i).Key + "\":" + JsonConvert.SerializeObject(formData.ElementAt(i).Value, Formatting.Indented) + ",";
                    }
                    else
                    {
                        var formElement = formData.ElementAt(i);
                        string val = formElement.Value;

                        //adding machines to the list
                        if (formElement.Key.Contains("MultipleMachineName"))
                        {
                            foreach (var machine in formElement.Value.Replace("\"", "").Split(" ").ToList())
                            {
                                Machine m = MachineService?.GetVirtualMachineByName(machine);
                                if (m != null && m.Name != null)
                                {
                                    machines.Add(m);
                                }
                            }
                        }
                        else if (formElement.Key.Contains("MachineName") || formElement.Key.Contains("Machine_Name"))
                        {
                            Machine m = MachineService?.GetVirtualMachineByName(formElement.Value);
                            if (m != null && m.Name != null)
                            {
                                machines.Add(m);
                            }
                        }

                        if (val.Contains("\""))
                        {
                            val = val.Replace("\"", "\\\"");
                        }
                        jsonString += "\"" + formElement.Key + "\":\"" + val + "\",";

                    }
                }
                jsonString = jsonString.Remove(jsonString.Length - 1, 1);
                jsonString += "}}";

                var result = AzureService?.RunPipeline(jsonString, PipeLineId ?? "");
                if (result == null || !result.IsSuccessStatusCode)
                {
                    notification = "Error: Unable to run pipeline!";
                }
                else
                {
                    notification = "Pipeline Run Success!";
                    ReserveMachines(machines);
                }
            }
            finally
            {
                isSubmitting = false;
            }
        }
        protected void ReserveMachines(List<Machine> machines)
        {
            foreach (var machine in machines)
            {
                if (machine.Status == "Available")
                {
                    string userId = HttpContextAccessor?.HttpContext?.User?.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "";
                    MachineService?.AssignUser(userId, HttpContextAccessor?.HttpContext?.User?.Identity?.Name ?? "", "Auto Assigned", DateTime.UtcNow.AddDays(2).AddMinutes(330), machine);
                }
            }
        }
        protected void clearNotification()
        {
            notification = string.Empty;
        }
        public async ValueTask DisposeAsync()
        {
            if (hubConnection != null)
            {
                await hubConnection.DisposeAsync();
            }
        }
        protected class Parameter
        {
            public string name { get; set; }
            public string displayName { get; set; }
            public string type { get; set; }
            public string Default { get; set; }
            public List<String?> values { get; set; }
        }

    }
}

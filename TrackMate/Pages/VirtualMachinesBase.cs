using Blazored.LocalStorage;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.SignalR.Client;
using System.Security.Claims;
using TrackMate.Models;
using TrackMate.Services;

namespace TrackMate.Pages
{
    public class VirtualMachinesBase : ComponentBase, IAsyncDisposable
    {
        [Inject]
        MachineService? MachineService { get; set; }

        [Inject]
        public UserService? UserService { get; set; }
        [Inject]
        public AzureService? AzureService { get; set; }

        [Inject]
        public NavigationManager? NavigationManager { get; set; }
        [Inject]
        public IConfiguration? configuration { get; set; }
        [Inject]
        public IHttpContextAccessor? HttpContextAccessor { get; set; }

        [Inject]
        public ILocalStorageService LocalStorageService { get; set; }

        protected Filter filter { get; set; } = new Filter();
        protected int appliedFilters = 0;

        public List<Machine>? MachinesList { get; set; }
        public IEnumerable<Machine>? AllMachinesList { get; set; }
        public List<Agent>? VirtualMachinesList { get; set; } = new List<Agent>();
        public IEnumerable<ApplicationUser>? UsersList { get; set; }
        protected DateTime endTime { get; set; } = DateTime.UtcNow.AddDays(2).AddMinutes(330);

        protected Machine? machine;

        protected string? userDet;
        protected string? userId;
        protected string? userName;
        protected string? machineId;
        protected string? comments;
        protected string? message = string.Empty;
        protected string? notification = string.Empty;

        protected string isModalActive = "";
        protected string? modalTitle { get; set; }
        protected bool isFilter = false;
        protected bool isAssign = false;
        protected bool isEditAssign = false;
        protected bool isSubmitting = false;
        private HubConnection hubConnection { get; set; }

        protected override async Task OnInitializedAsync()
        {
            base.OnInitialized();
            try
            {
                UsersList = UserService?.GetUsers().ToList();
                AllMachinesList = MachineService?.GetVirtualMachines().OrderBy(o => o.Name).ToList();
                MachinesList = AllMachinesList?.ToList();
                VirtualMachinesList = AzureService?.GetAgentsWithCapability();
            }
            catch (Exception)
            {
                UsersList = new List<ApplicationUser>();
                MachinesList = new List<Machine>();
            }

            //connecting to the blazor hub 
            if (configuration != null)
            {
                string TokenValue = await LocalStorageService.GetItemAsStringAsync("TokenValue");
                hubConnection = new HubConnectionBuilder()
                   .WithUrl(configuration["HubUrl"], options =>
                   {
                       options.AccessTokenProvider = () => Task.FromResult(TokenValue ?? null);
                   })
                   .Build();

                hubConnection.On<string>("MachineLoaded", OnMachineLoaded);
                hubConnection.On<string>("UserLoaded", OnUserLoaded);
                hubConnection.On<string>("privateNotification", OnNotificationReceived);

                await hubConnection.StartAsync();
            }
        }
        private void OnNotificationReceived(string message)
        {
            OnMachineLoaded(null);
        }

        private void OnMachineLoaded(string machine)
        {
            AllMachinesList = MachineService?.GetVirtualMachines().OrderBy(o => o.Name).ToList();
            MachinesList = AllMachinesList?.ToList();
            StateHasChanged();
        }

        private void OnUserLoaded(string id)
        {
            UsersList = UserService?.GetUsers().ToList();
            StateHasChanged();
        }
        protected void OpenFilter()
        {
            isFilter = true;
            isModalActive = "page-container";
        }
        protected void CloseFilter()
        {
            isFilter = false;
            isModalActive = "";
        }
        protected void AssignMachine(string? id)
        {
            machineId = id ?? "";
            comments = "None";
            modalTitle = "Checkout To User";
            isModalActive = "page-container";
            endTime = DateTime.UtcNow.AddDays(2).AddMinutes(330);
            isAssign = true;
        }
        protected void EditAssignedMachine(Machine machine)
        {
            this.machine = machine;
            comments = "None";
            modalTitle = "Edit Machine Assignment";
            isModalActive = "page-container";
            isEditAssign = true;
        }
        protected void ExtendTimeout()
        {
            if (isSubmitting) return;
            isSubmitting = true;

            try
            {
                if (machine != null)
                {
                    machine.LastAccessed = DateTime.UtcNow;
                    string response = MachineService?.UpdateAssignedMachine(machine) ?? "Failed Operation!";
                    notification = response;
                }

            }
            catch (Exception)
            {
                message = "Error! Please Retry";
            }
            finally
            {
                closeModal();
            }
        }
        protected void AssignToUser()
        {
            if (isSubmitting) return;
            isSubmitting = true;

            try
            {
                if (userDet != null)
                {
                    userId = userDet.Split(" ")[0];
                    userName = userDet.Split(" ")[1];
                    Machine? SelectedMachine = MachineService?.GetMachineById(machineId ?? "");
                    if (SelectedMachine != null)
                    {
                        SelectedMachine.LastAccessed = DateTime.UtcNow;

                        if (SelectedMachine.Status == "Available")
                        {
                            string? response = MachineService?.AssignUser(userId, userName, comments ?? "None", endTime, SelectedMachine);
                            notification = response;
                        }
                        else
                            message = "Selected Machine is not Available!";
                    }
                }
                else
                {
                    message = "Please select the user from the list to assign!";
                }
            }
            catch (Exception)
            {
                message = "Error! Please Retry";
            }
            finally
            {
                closeModal();
            }
        }


        protected void RevokeMachine(Machine machine)
        {

            if (isSubmitting) return;

            isSubmitting = true;

            try
            {
                machine.LastAccessed = DateTime.UtcNow;
                string? response = MachineService?.RevokeUser(machine);
                notification = response;
            }
            catch (Exception)
            {
                message = "Error! Please Retry";
            }
            finally { closeModal(); }

        }

        protected void ClearFilter()
        {
            if (isSubmitting) return;
            isSubmitting = true;
            try
            {
                filter = new Filter();
                appliedFilters = 0;
                AllMachinesList = MachineService?.GetVirtualMachines().OrderBy(o => o.Name).ToList();
                MachinesList = AllMachinesList?.ToList();
            }
            catch (Exception)
            {
                isSubmitting = false;
                CloseFilter();
            }
            finally
            {
                isSubmitting = false;
                CloseFilter();
            }

        }
        protected void ApplyFilter()
        {
            if (isSubmitting) return;
            isSubmitting = true;

            try
            {
                appliedFilters = 0;
                if (filter.MachinePurpose != "Any")
                {
                    appliedFilters += 1;
                    VirtualMachinesList = VirtualMachinesList?.FindAll(o => o.Capability?.Machine_Purpose == filter.MachinePurpose).ToList();
                }
                if (filter.NoOfProcessors != "Any")
                {
                    appliedFilters += 1;
                    VirtualMachinesList = VirtualMachinesList?.FindAll(o => o.Capability?.NUMBER_OF_PROCESSORS == filter.NoOfProcessors).ToList();
                }

                if (appliedFilters > 0)
                {
                    List<int>? machineIds = VirtualMachinesList?.Select(l => l.Id).ToList();
                    MachinesList = new List<Machine>();
                    if (machineIds != null)
                    {
                        foreach (var id in machineIds)
                        {
                            Machine? machine = AllMachinesList?.Where(o => o.AgentId == id.ToString()).FirstOrDefault() ?? null;
                            if (machine != null)
                            {
                                MachinesList.Add(machine);
                            }

                        }
                    }
                    VirtualMachinesList = AzureService?.GetAgentsWithCapability();
                }
                else
                {
                    MachinesList = AllMachinesList?.ToList();
                }
            }
            catch (Exception)
            {
                isSubmitting = false;
                CloseFilter();
            }
            finally
            {
                isSubmitting = false;
                CloseFilter();
            }

        }
        protected string DisplayCapabilityToolTip(string AgentId)
        {
            Agent hoverAgent = VirtualMachinesList.Find(o => o.Id.ToString() == AgentId);
            string content = "";
            if (hoverAgent != null)
            {
                content += "Agent Version: " + hoverAgent.Capability.AgentVersion;
                content += "\nAgent OS: " + hoverAgent.Capability.AgentOS;
                content += "\nAgent OS Architecture: " + hoverAgent.Capability.AgentOSArchitecture;
                content += "\nAgent OS Version: " + hoverAgent.Capability.AgentOSVersion;
                content += "\nMachine Purpose: " + hoverAgent.Capability.Machine_Purpose;
                content += "\nNo of Processors: " + hoverAgent.Capability.NUMBER_OF_PROCESSORS;
                content += "\nProcessor Architecture: " + hoverAgent.Capability.PROCESSOR_ARCHITECTURE;
                content += "\nProcessor Level: " + hoverAgent.Capability.PROCESSOR_LEVEL;

            }
            return content == string.Empty ? "None" : content;
        }

        protected void closeModal()
        {
            isAssign = false;
            isModalActive = "";
            isSubmitting = false;
            isEditAssign = false;
        }
        protected void clearNotification()
        {
            notification = string.Empty;
        }
        public class Filter
        {
            public string MachinePurpose { get; set; } = "Any";
            public string NoOfProcessors { get; set; } = "Any";
            public string FilterType { get; set; } = "MachinePurpose";

        }
        public async ValueTask DisposeAsync()
        {
            if (hubConnection != null)
            {
                await hubConnection.DisposeAsync();
            }
        }
    }
}

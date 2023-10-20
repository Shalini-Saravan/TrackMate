using Blazored.LocalStorage;
using BlazorServerAppWithIdentity.Models;
using BlazorServerAppWithIdentity.Services;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.SignalR.Client;
using System.Security.Claims;

namespace BlazorServerAppWithIdentity.Pages
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
        protected Boolean isFilter = false;
        protected Boolean isAssign = false;
        protected Boolean isEditAssign = false;
        protected Boolean isSubmitting = false;
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
                hubConnection.On<String>("UserLoaded", OnUserLoaded);
                hubConnection.On<String>("privateNotification", OnNotificationReceived);

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
            this.isModalActive = "page-container";
        }
        protected void CloseFilter()
        {
            isFilter = false;
            this.isModalActive = "";
        }
        protected void AssignMachine(string? id)
        {
            this.machineId = id ?? "";
            this.comments = "None";
            this.modalTitle = "Checkout To User";
            this.isModalActive = "page-container";
            this.endTime = DateTime.UtcNow.AddDays(2).AddMinutes(330);
            this.isAssign = true;
        }
        protected void EditAssignedMachine(Machine machine)
        {
            this.machine = machine;
            this.comments = "None";
            this.modalTitle = "Edit Machine Assignment";
            this.isModalActive = "page-container";
            this.isEditAssign = true;
        }
        protected void ExtendTimeout()
        {
            if (isSubmitting) return;
            this.isSubmitting = true;

            try
            {
                if (machine != null)
                {
                    machine.LastAccessed = DateTime.UtcNow;
                    string response = MachineService?.UpdateAssignedMachine(machine) ?? "Failed Operation!";
                    this.notification = response;
                }

            }
            catch (Exception)
            {
                this.message = "Error! Please Retry";
            }
            finally
            {
                closeModal();
            }
        }
        protected void AssignToUser()
        {
            if (isSubmitting) return;
            this.isSubmitting = true;

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
                            this.notification = response;
                        }
                        else
                            this.message = "Selected Machine is not Available!";
                    }
                }
                else
                {
                    this.message = "Please select the user from the list to assign!";
                }
            }
            catch (Exception)
            {
                this.message = "Error! Please Retry";
            }
            finally
            {
                closeModal();
            }
        }


        protected void RevokeMachine(Machine machine)
        {

            if (isSubmitting) return;

            this.isSubmitting = true;

            try
            {
                machine.LastAccessed = DateTime.UtcNow;
                string? response = MachineService?.RevokeUser(machine);
                this.notification = response;
            }
            catch (Exception)
            {
                this.message = "Error! Please Retry";
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
                this.CloseFilter();
            }
            finally
            {
                isSubmitting = false;
                this.CloseFilter();
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
                this.CloseFilter();
            }
            finally
            {
                isSubmitting = false;
                this.CloseFilter();
            }

        }
        protected String DisplayCapabilityToolTip(string AgentId)
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
            return content == string.Empty? "None" : content;
        }

        protected void closeModal()
        {
            this.isAssign = false;
            this.isModalActive = "";
            this.isSubmitting = false;
            this.isEditAssign = false;
        }
        protected void clearNotification()
        {
            this.notification = string.Empty;
        }
        public class Filter
        {
            public string MachinePurpose { get; set; } = "Any";
            public string NoOfProcessors { get; set; } = "Any";
            public string FilterType { get; set; } = "MachinePurpose";
           
        }
        public async ValueTask DisposeAsync()
        {
            await hubConnection.DisposeAsync();
        }
    }
}

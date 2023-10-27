using Blazored.LocalStorage;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.SignalR.Client;
using System.Net;
using System.Security.Claims;
using TrackMate.Models;
using TrackMate.Services;

namespace TrackMate.Pages
{
    public class PhysicalMachinesBase : ComponentBase, IAsyncDisposable
    {
        [Inject]
        public MachineService? MachineService { get; set; }

        [Inject]
        public UserService? UserService { get; set; }

        [Inject]
        public NavigationManager? NavigationManager { get; set; }
        [Inject]
        public IHttpContextAccessor? HttpContextAccessor { get; set; }


        [Inject]
        public IConfiguration? configuration { get; set; }
        [Inject]
        public ILocalStorageService? LocalStorage { get; set; }
        [Inject]
        public ILocalStorageService LocalStorageService { get; set; }
        public IEnumerable<Machine>? MachinesList { get; set; }
        public IEnumerable<ApplicationUser>? UsersList { get; set; }

        protected Machine? machine;

        protected string? userId;
        protected string? userName;
        protected string? userDet;
        protected string? machineId;
        protected string? comments;
        protected string? message = string.Empty;
        protected string? notification = string.Empty;
        protected DateTime endTime { get; set; } = DateTime.UtcNow.AddDays(2).AddMinutes(330);

        protected string isModalActive = "";
        protected string? modalTitle { get; set; }

        protected bool isAdd = false;
        protected bool isAssign = false;
        protected bool isSubmitting = false;
        protected bool isEditAssign = false;
        protected string conId;
        private HubConnection hubConnection { get; set; }

        protected override async Task OnInitializedAsync()
        {
            base.OnInitialized();

            try
            {
                UsersList = UserService?.GetUsers().ToList();
                MachinesList = MachineService?.GetMachines().ToList();
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
            MachinesList = MachineService?.GetMachines().ToList();
            StateHasChanged();
        }

        private void OnUserLoaded(string id)
        {
            UsersList = UserService?.GetUsers().ToList();
            StateHasChanged();
        }

        protected void AddNew()
        {
            machine = new Machine();
            machine.Purpose = "None";
            machine.Comments = "None";
            modalTitle = "Add New Machine";
            isModalActive = "page-container";
            isAdd = true;
        }

        protected void EditMachine(Machine machine)
        {
            this.machine = machine;
            modalTitle = "Edit Machine";
            isModalActive = "page-container";
            isAdd = true;
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
                            string response = MachineService?.AssignUser(userId, userName, comments ?? "None", endTime, SelectedMachine) ?? "";
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
                machine.Comments = "None";
                string response = MachineService?.RevokeUser(machine) ?? "Failed Operation!";
                notification = response;
            }
            catch (Exception)
            {
                message = "Error! Please Retry";
            }
            finally { closeModal(); }

        }


        protected void closeModal()
        {
            isAdd = false;
            isAssign = false;
            isModalActive = "";
            isSubmitting = false;
            isEditAssign = false;
        }
        protected void clearNotification()
        {
            notification = string.Empty;
        }


        protected void SaveMachine()
        {
            if (isSubmitting) return;

            isSubmitting = true;
            try
            {
                if (machine != null)
                {
                    if (modalTitle == "Edit Machine")
                    {
                        machine.LastAccessed = DateTime.UtcNow;
                        string response = MachineService?.UpdateMachine(machine) ?? "Failed Operation!";
                        notification = response;
                    }
                    else
                    {
                        machine.Status = "Available";
                        machine.Type = "Physical";
                        machine.LastAccessed = DateTime.UtcNow;
                        string response = MachineService?.AddMachine(machine) ?? "Failed Operation!";
                        notification = response;
                    }
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

        protected void DeleteMachine(string? id)
        {
            if (isSubmitting || id == null) return;

            isSubmitting = true;
            try
            {
                string response = MachineService?.RemoveMachine(id) ?? "Failed Operation!";
                notification = response;
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

        public async ValueTask DisposeAsync()
        {
            if (hubConnection != null)
            {
                await hubConnection.DisposeAsync();
            }
        }
    }
}

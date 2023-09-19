using BlazorServerAppWithIdentity.Models;
using BlazorServerAppWithIdentity.Services;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.SignalR.Client;
using System.Security.Claims;

namespace BlazorServerAppWithIdentity.Pages
{
    public class PhysicalMachinesBase : ComponentBase
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
        IGlobalStateService? GlobalStateService { get; set; }
        [Inject]
        public IConfiguration? configuration { get; set; }
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

        protected Boolean isAdd = false;
        protected Boolean isAssign = false;
        protected Boolean isSubmitting = false;

        protected override async Task OnInitializedAsync()
        {
            base.OnInitialized();
            if (GlobalStateService?.PAT == null && (HttpContextAccessor?.HttpContext?.User?.Identity?.IsAuthenticated ?? false))
            {
                string id = HttpContextAccessor?.HttpContext?.User?.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "";
                ApplicationUser? appuser = UserService?.GetUserById(id);
                if (appuser != null && appuser.PAT == null)
                {
                    NavigationManager?.NavigateTo("/pat");
                }
                else if (GlobalStateService != null)
                {
                    GlobalStateService.PAT = appuser?.PAT ?? "";
                }
            }
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
                var hubConnection = new HubConnectionBuilder()
                    .WithUrl(configuration["HubUrl"])
                    .Build();

                hubConnection.On<String>("MachineLoaded", OnMachineLoaded);
                hubConnection.On<String>("UserLoaded", OnUserLoaded);

                await hubConnection.StartAsync();
            }
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
            this.modalTitle = "Add New Machine";
            this.isModalActive = "page-container";
            this.isAdd = true;
        }

        protected void EditMachine(Machine machine)
        {
            this.machine = machine;
            this.modalTitle = "Edit Machine";
            this.isModalActive = "page-container";
            this.isAdd = true;
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
                            string response = MachineService?.AssignUser(userId, userName, comments ?? "None", endTime, SelectedMachine) ?? "";
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
                machine.Comments = "None";
                string response = MachineService?.RevokeUser(machine) ?? "Failed Operation!";
                this.notification = response;
            }
            catch (Exception)
            {
                this.message = "Error! Please Retry";
            }
            finally { closeModal(); }

        }


        protected void closeModal()
        {
            this.isAdd = false;
            this.isAssign = false;
            this.isModalActive = "";
            this.isSubmitting = false;
        }
        protected void clearNotification()
        {
            this.notification = string.Empty;
        }


        protected void SaveMachine()
        {
            if (isSubmitting) return;

            this.isSubmitting = true;
            try
            {
                if (machine != null)
                {
                    if (modalTitle == "Edit Machine")
                    {
                        machine.LastAccessed = DateTime.UtcNow;
                        string response = MachineService?.UpdateMachine(machine) ?? "Failed Operation!";
                        this.notification = response;
                    }
                    else
                    {
                        machine.Status = "Available";
                        machine.Type = "Physical";
                        machine.LastAccessed = DateTime.UtcNow;
                        string response = MachineService?.AddMachine(machine) ?? "Failed Operation!";
                        this.notification = response;
                    }
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

        protected void DeleteMachine(string? id)
        {
            if (isSubmitting || id == null) return;

            this.isSubmitting = true;
            try
            {
                string response = MachineService?.RemoveMachine(id) ?? "Failed Operation!";
                this.notification = response;
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
    }
}

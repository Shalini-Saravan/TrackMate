using Microsoft.AspNetCore.Components;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.SignalR.Client;
using System.Reflection.PortableExecutable;
using System.Security.Claims;
using System.Net.Security;
using Newtonsoft.Json.Linq;
using Azure;
using Blazored.LocalStorage;
using TrackMate.Models;
using TrackMate.Services;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.JSInterop;

namespace TrackMate.Pages
{
    public class UsersBase : ComponentBase, IAsyncDisposable
    {
        [Inject]
        public UserService? UserService { get; set; }
        [Inject]
        public AccountService? AccountService { get; set; }
        [Inject]
        public MachineService? MachineService { get; set; }
        [Inject]
        public NavigationManager? NavigationManager { get; set; }


        [Inject]
        public IHttpContextAccessor? HttpContextAccessor { get; set; }
        [Inject]
        public IConfiguration? configuration { get; set; }
        [Inject]
        public ILocalStorageService LocalStorageService { get; set; }
        [Inject]
        public IJSRuntime JSRuntime { get; set; }
        public IEnumerable<ApplicationUser>? UsersList { get; set; }


        public List<ApplicationRole>? rolesCollection;
        protected List<Models.Machine>? _machines;


        protected User? user;

        protected User? userEdit;
        protected string? uid;
        protected ClaimsPrincipal? userid;
        protected ApplicationUser? SelectedUser;
        protected string? modalTitle { get; set; }
        protected bool isAdd = false;
        protected bool isEdit = false;
        protected bool isView = false;

        protected string? message { get; set; }
        protected string isModalActive = "";
        protected bool isSubmitting = false;
        protected ApplicationUser? appuser;
        private HubConnection hubConnection { get; set; }

        protected async override void OnInitialized()
        {
            base.OnInitialized();

            try
            {
                UsersList = UserService?.GetUsers().ToList().OrderBy(o => o.Name);
                rolesCollection = UserService?.GetRoles().ToList();
            }
            catch (Exception)
            {
                UsersList = new List<ApplicationUser>();
                rolesCollection = new List<ApplicationRole>();
            }

            if (configuration != null)
            {
                string TokenValue = await LocalStorageService.GetItemAsStringAsync("TokenValue");
                hubConnection = new HubConnectionBuilder()
                   .WithUrl(configuration["HubUrl"], options =>
                   {
                       options.AccessTokenProvider = () => Task.FromResult(TokenValue ?? null);
                   })
                   .Build();

                hubConnection.On<string>("UserLoaded", OnUserLoaded);
                hubConnection.StartAsync();
                
            }

        }
        
        private void OnUserLoaded(string user)
        {
            try
            {
                UsersList = UserService?.GetUsers().ToList().OrderBy(o => o.Name);
            }
            catch (Exception)
            {
                UsersList = new List<ApplicationUser>();
            }
            StateHasChanged();
        }
        protected void ReloadUsers()
        {
            try
            {
                UsersList = UserService?.GetUsers().ToList().OrderBy(o => o.Name);
            }
            catch (Exception)
            {
                UsersList = new List<ApplicationUser>();
            }
        }

        protected void closeModal()
        {
            isAdd = false;
            isEdit = false;
            isView = false;
            isModalActive = "";
            isSubmitting = false;
        }

        protected void AddNew()
        {
            user = new User();
            modalTitle = "Add New User";
            isAdd = true;
            isEdit = false;
            isModalActive = "page-container";
            message = null;
        }

        protected void EditUser(ApplicationUser _user)
        {
            appuser = _user;
            uid = _user.Id.ToString();
            userEdit = new User();
            userEdit.Name = _user.Name ?? "";
            userEdit.Email = _user.Email;
            userEdit.Designation = _user.Designation;
            userEdit.Department = _user.Department ?? "";
            userEdit.Team = _user.Team ?? "";
            userEdit.Roles = AccountService.GetUserRole(_user.Roles.FirstOrDefault().ToString());
            modalTitle = "Edit User";
            isEdit = true;
            isAdd = false;
            isModalActive = "page-container";
            message = null;
        }

        protected void SaveUser()
        {
            if (isSubmitting) return; isSubmitting = true;
            try
            {
                if (user != null)
                {
                    string? result = UserService?.AddUser(user);
                    message = result;
                    ReloadUsers();
                }
                else
                {
                    message = "Failed Operation!";
                }
            }
            catch (Exception)
            {
                message = "Error: Failed to add new user!";
            }
            finally
            {
                closeModal();
                isSubmitting = false;
            }

        }
        protected void AddRole(ApplicationRole role)
        {
            try
            {
                string? response = UserService?.AddRole(role);
                message = response;
            }
            catch (Exception)
            {
                message = "Error: Failed to edit user!";
            }

        }
        protected void SaveEditUser()
        {
            if (isSubmitting) return; isSubmitting = true;
            try
            {
                if (userEdit != null)
                {
                    string? response = UserService?.EditUser(userEdit, uid ?? "");

                    message = response;
                    ReloadUsers();
                }
            }
            catch (Exception)
            {
                message = "Error: Failed to edit user!";
            }
            finally
            {
                closeModal();
                appuser = null;
                isSubmitting = false;
            }

        }
        protected void DeleteUser()
        {
            try
            {
                if (appuser != null)
                {
                    string? response = UserService?.DeleteUser(appuser);
                    message = response;
                    ReloadUsers();
                }
            }
            catch (Exception)
            {
                message = "Unable to delete user!";
            }
            finally
            {
                closeModal();
                appuser = null;
                isSubmitting = false;
            }
        }
        protected void ViewMachine(string id)
        {
            _machines = MachineService?.GetMachinesByUserId(id);
            isView = true;
            modalTitle = "Machines Used";
            isModalActive = "page-container";
        }

        protected void RevokeMachine(Models.Machine machine)
        {
            try
            {
                MachineService?.RevokeUser(machine);
                _machines = MachineService?.GetMachinesByUserId(machine.UserId ?? "");
                isView = true;
                isModalActive = "page-container";
            }
            catch (Exception)
            {
                message = "Unable to revoke machine!";
            }
        }
        protected void clearMessage()
        {
            message = null;
        }
        public void Esc(KeyboardEventArgs e)
        {
            
            if (e.Code == "Escape" || e.Key == "Escape")
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

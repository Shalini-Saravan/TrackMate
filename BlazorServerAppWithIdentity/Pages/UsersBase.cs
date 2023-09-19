using BlazorServerAppWithIdentity.Models;
using Microsoft.AspNetCore.Components;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using BlazorServerAppWithIdentity.Services;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.SignalR.Client;
using System.Reflection.PortableExecutable;
using System.Security.Claims;
using System.Net.Security;
using Newtonsoft.Json.Linq;
using Azure;

namespace BlazorServerAppWithIdentity.Pages
{
    public class UsersBase : ComponentBase
    {
        [Inject]
        public UserService? UserService { get; set; }
        [Inject]
        public MachineService? MachineService { get; set; }
        [Inject]
        public NavigationManager? NavigationManager { get; set; }

        [Inject]
        IGlobalStateService? GlobalStateService { get; set; }
        [Inject]
        public IHttpContextAccessor? HttpContextAccessor { get; set; }
        [Inject]
        public IConfiguration? configuration { get; set; }

        public IEnumerable<ApplicationUser>? UsersList { get; set; }


        public List<ApplicationRole>? rolesCollection;
        protected List<Models.Machine>? _machines;


        protected User? user;

        protected User? userEdit;
        protected string? uid;
        protected ClaimsPrincipal? userid;
        protected ApplicationUser? SelectedUser;
        protected string? modalTitle { get; set; }
        protected Boolean isAdd = false;
        protected Boolean isEdit = false;
        protected Boolean isView = false;

        protected string? message { get; set; }
        protected string isModalActive = "";
        protected bool isSubmitting = false;
        protected ApplicationUser? appuser;
        

        protected override void OnInitialized()
        {
            base.OnInitialized();
            
            try
            {
                UsersList = UserService?.GetUsers().ToList();
                rolesCollection = UserService?.GetRoles().ToList();
            }
            catch (Exception)
            {
                UsersList = new List<ApplicationUser>();
                rolesCollection = new List<ApplicationRole>();
            }

            if (configuration != null)
            {
                var hubConnection = new HubConnectionBuilder()
                    .WithUrl(configuration["HubUrl"])
                    .Build();

                hubConnection.On<String>("UserLoaded", OnUserLoaded);
                hubConnection.StartAsync();
            }

        }

        private void OnUserLoaded(String user)
        {
            try
            {
                UsersList = UserService?.GetUsers().ToList();
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
                UsersList = UserService?.GetUsers().ToList();
            }
            catch (Exception)
            {
                UsersList = new List<ApplicationUser>();
            }
        }

        protected void closeModal()
        {
            this.isAdd = false;
            this.isEdit = false;
            this.isView = false;
            this.isModalActive = "";
            this.isSubmitting = false;
        }

        protected void AddNew()
        {
            user = new User();
            this.modalTitle = "Add New User";
            this.isAdd = true;
            this.isEdit = false;
            this.isModalActive = "page-container";
            this.message = null;
        }

        protected void EditUser(ApplicationUser _user)
        {
            this.appuser = _user;
            uid = _user.Id.ToString();
            userEdit = new User();
            userEdit.Name = _user.Name ?? "";
            userEdit.Email = _user.Email;
            userEdit.Department = _user.Department ?? "";
            userEdit.Team = _user.Team ?? "";
            this.modalTitle = "Edit User";
            this.isEdit = true;
            this.isAdd = false;
            this.isModalActive = "page-container";
            this.message = null;
        }

        protected void SaveUser()
        {
            if (isSubmitting) return; this.isSubmitting = true;
            try
            {
                if (user != null)
                {
                    string? result = UserService?.AddUser(user);
                    this.message = result;
                    ReloadUsers();
                }
                else
                {
                    this.message = "Failed Operation!";
                }
            }
            catch(Exception)
            {
                this.message = "Error: Failed to add new user!";
            }
            finally
            {
                closeModal();
                this.isSubmitting = false;
            }

        }
        protected void AddRole(ApplicationRole role)
        {
            try
            {
                string? response = UserService?.AddRole(role);
                this.message = response;
            }
            catch (Exception)
            {
                this.message = "Error: Failed to edit user!";
            }

        }
        protected void SaveEditUser()
        {
            if (isSubmitting) return; this.isSubmitting = true;
            try
            {
                if (userEdit != null)
                {
                    string? response = UserService?.EditUser(userEdit, uid?? "");

                    this.message = response;
                    ReloadUsers();
                }
            }
            catch (Exception)
            {
                this.message = "Error: Failed to edit user!";
            }
            finally
            {
                closeModal();
                this.appuser = null;
                this.isSubmitting = false;
            }

        }
        protected void DeleteUser()
        {
            try
            {
                if (appuser != null)
                {
                    string? response = UserService?.DeleteUser(appuser);
                    this.message = response;
                    ReloadUsers();
                }
            }
            catch (Exception)
            {
                this.message = "Unable to delete user!";
            }
            finally
            {
                closeModal();
                this.appuser = null;
                this.isSubmitting = false;
            }
        }
        protected void ViewMachine(String id)
        {
            _machines =  MachineService?.GetMachinesByUserId(id);
            this.isView = true;
            this.modalTitle = "Machines Used";
            this.isModalActive = "page-container";
        }

        protected void RevokeMachine(Models.Machine machine)
        {
            try
            {
                MachineService?.RevokeUser(machine);
                _machines = MachineService?.GetMachinesByUserId(machine.UserId?? "");
                this.isView = true;
                this.isModalActive = "page-container";
            }
            catch(Exception)
            {
                this.message = "Unable to revoke machine!";
            }
        }
        protected void clearMessage()
        {
            this.message = null;
        }

    }
}
